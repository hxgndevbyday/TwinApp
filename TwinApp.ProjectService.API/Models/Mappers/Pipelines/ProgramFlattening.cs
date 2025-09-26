namespace TwinApp.ProjectService.API.Models.Mappers.Pipelines;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;


public static class ProgramFlattening
{
    // Entry point: flatten a program section (a BsonDocument that contains $values)
    public static List<ProjectEntityDto> FlattenProgramSection(BsonDocument programsSectionDoc, string referenceId)
    {
        var entities = new List<ProjectEntityDto>();
        var legacyIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
        
        // 1) Find top-level programs array (robust handling)
        BsonArray programsArray;
        if (programsSectionDoc["Values"].AsBsonArray.Contains("$values")) programsArray = programsSectionDoc["$values"].AsBsonArray;
        else if (programsSectionDoc.IsBsonArray) programsArray = programsSectionDoc.AsBsonArray;
        else return entities; // nothing to do

        // Helper: recursively create entities and populate legacy map
        void FirstPassCreate(BsonArray array, string parentEntityId)
        {
            foreach (var item in array)
            {
                if (!item.IsBsonDocument) continue;

                var doc = item.AsBsonDocument;

                // generate entity id
                var newEntityId = Guid.NewGuid().ToString();

                // record legacy $id if present (top-level item $id)
                if (doc.Contains("$id"))
                {
                    var legacy = doc["$id"].ToString();
                    legacyIdMap[legacy] = newEntityId;
                }

                // optional: also record legacy BfKey inner unique key if exists (Id._uniqueKey)
                string legacyKey = null;
                if (doc.Contains("Id") && doc["Id"].IsBsonDocument)
                {
                    var idDoc = doc["Id"].AsBsonDocument;
                    if (idDoc.Contains("_uniqueKey")) legacyKey = idDoc["_uniqueKey"].ToString();
                    // you may want to expose legacyKey in components later
                }

                // determine friendly entity type
                var rawType = doc.Contains("$type") ? doc["$type"].ToString() : null;
                var entityType = SimplifyType(rawType) ?? "ProgramItem";

                // components: shallow conversion but do NOT expand nested $values arrays here
                var components = ToDictionaryShallow(doc);

                // add metadata fields (keep legacy info)
                if (!components.ContainsKey("LegacyType") && rawType != null)
                    components["LegacyType"] = rawType;
                if (!string.IsNullOrEmpty(legacyKey))
                    components["LegacyKey"] = legacyKey;
                if (doc.Contains("$id"))
                    components["LegacyId"] = doc["$id"].ToString();

                var dto = new ProjectEntityDto(
                    ProjectId: referenceId,
                    EntityType: entityType,
                    EntityId: newEntityId,
                    ParentId: parentEntityId,
                    Components: components
                );

                entities.Add(dto);

                // Recurse into children arrays (if present). Use doc.Elements to find children arrays.
                foreach (var nested in GetNestedArrays(doc))
                {
                    FirstPassCreate(nested, newEntityId);
                }
            }
        }

        // For each top-level program create a root program node and then create its children
        foreach (var programItem in programsArray)
        {
            if (!programItem.IsBsonDocument) continue;
            var programDoc = programItem.AsBsonDocument;

            var programId = Guid.NewGuid().ToString();
            if (programDoc.Contains("$id")) legacyIdMap[programDoc["$id"].ToString()] = programId;

            var programType = SimplifyType(programDoc.GetValue("$type", BsonNull.Value).ToString()) ?? "Program";
            var programComponents = ToDictionaryShallow(programDoc);

            // Create root Program entity
            var programDto = new ProjectEntityDto(referenceId, programType, programId, ParentId: null, programComponents);
            entities.Add(programDto);

            // If program has Children array, recurse creating children with parent = programId
            var childs = GetNestedArrays(programDoc);
            foreach (var childArray in childs)
                FirstPassCreate(childArray, programId);
        }

        // 2) Second pass: resolve $ref and clean Components
        foreach (var e in entities)
        {
            ResolveRefsInComponents(e.Components, legacyIdMap);
        }

        return entities;
    }
    
    // Helper: returns nested BsonArray children found in a document
    private static IEnumerable<BsonArray> GetNestedArrays(BsonDocument doc)
    {
        foreach (var el in doc.Elements)
        {
            var v = el.Value;
            if (v.IsBsonDocument && v.AsBsonDocument.Contains("$values"))
                yield return v.AsBsonDocument["$values"].AsBsonArray;
            else if (v.IsBsonArray)
                yield return v.AsBsonArray;
        }
    }

    // Shallow conversion: convert scalar fields and small nested objects to CLR, but leave $values arrays out.
    private static IDictionary<string, object> ToDictionaryShallow(BsonDocument doc)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var el in doc.Elements)
        {
            // skip structural helpers
            if (el.Name == "$id" || el.Name == "$type") continue;

            var v = el.Value;

            // skip big nested arrays (we'll handle as children)
            if (v.IsBsonDocument && v.AsBsonDocument.Contains("$values")) 
                continue;
            if (v.IsBsonArray)
            {
                var arr = v.AsBsonArray;

                // Case 1: array of primitives → keep as list of CLR values
                if (arr.All(a => !a.IsBsonDocument))
                {
                    dict[el.Name] = arr.Select(ConvertBsonValue).ToList();
                }
                else
                {
                    // Case 2: array of documents → recurse
                    var children = new List<Dictionary<string, object>>();

                    foreach (var child in arr)
                    {
                        if (child.IsBsonDocument)
                        {
                            children.Add(child.AsBsonDocument.ToDictionaryRecursive());
                        }
                        else
                        {
                            children.Add(new Dictionary<string, object>
                            {
                                { "Value", ConvertBsonValue(child) ?? DBNull.Value }
                            });
                        }
                    }

                    dict[el.Name] = children;
                }

                continue;
            }


            // convert scalar/doc to CLR recursively (safe)
            dict[el.Name] = ConvertBsonValue(v) ?? DBNull.Value;
        }

        return dict;
    }

    // Convert BsonValue -> CLR (recursive for documents/arrays)
    private static object? ConvertBsonValue(BsonValue v)
    {
        if (v == null || v.IsBsonNull) return null;
        if (v.IsString) return v.AsString;
        if (v.IsInt32) return v.AsInt32;
        if (v.IsInt64) return v.AsInt64;
        if (v.IsDouble) return v.AsDouble;
        if (v.IsBoolean) return v.AsBoolean;
        if (v.IsObjectId) return v.AsObjectId.ToString();
        if (v.IsBsonDateTime) return v.AsBsonDateTime.ToUniversalTime();
        if (v.IsDecimal128) return Decimal128.ToDecimal(v.AsDecimal128); // or ToString() if precision issues
        if (v.IsBsonBinaryData) return v.AsBsonBinaryData.Bytes;

        if (v.IsBsonDocument)
        {
            var sub = v.AsBsonDocument;
            // special-case $ref objects: { "$ref": "15432" }
            if (sub.Contains("$ref") && sub.ElementCount == 1)
                return new Dictionary<string, object> { ["$ref"] = sub["$ref"].ToString() };

            // special-case BfKey objects (Id) with _uniqueKey
            if (sub.Contains("_uniqueKey"))
                return sub["_uniqueKey"].ToString();

            // generic conversion
            var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var el in sub.Elements)
                d[el.Name] = ConvertBsonValue(el.Value) ?? DBNull.Value;
            return d;
        }

        if (v.IsBsonArray)
            return v.AsBsonArray.Select(ConvertBsonValue).ToList();

        return v.ToString();
    }

    // Replace any $ref placeholders in components with mapped new entityId when possible
    private static void ResolveRefsInComponents(IDictionary<string, object> components, Dictionary<string, string> legacyIdMap)
    {
        var keys = components.Keys.ToList();
        foreach (var key in keys)
        {
            var val = components[key];

            if (val is IDictionary<string, object> dictVal)
            {
                // if { "$ref": "15432" } pattern
                if (dictVal.TryGetValue("$ref", out var refObj) && refObj is string legacyRef)
                {
                    if (legacyIdMap.TryGetValue(legacyRef, out var newId))
                        components[key] = newId; // replace with new entity id
                    else
                        components[key] = legacyRef; // fallback to legacy ref string
                    continue;
                }

                // recurse into sub-dictionaries
                ResolveRefsInComponents(dictVal, legacyIdMap);
            }
            else if (val is IEnumerable listVal && !(val is string))
            {
                var newList = new List<object>();
                var changed = false;
                foreach (var v in listVal)
                {
                    if (v is IDictionary<string, object> innerDict && innerDict.TryGetValue("$ref", out var r) && r is string lr)
                    {
                        changed = true;
                        if (legacyIdMap.TryGetValue(lr, out var nid)) newList.Add(nid);
                        else newList.Add(lr);
                    }
                    else if (v is IDictionary<string, object> inner)
                    {
                        ResolveRefsInComponents(inner, legacyIdMap);
                        newList.Add(inner);
                    }
                    else
                        newList.Add(v);
                }
                if (changed)
                    components[key] = newList;
            }
            else
            {
                // scalar, nothing to do
            }
        }
    }

    // Simplify full $type to last token
    private static string? SimplifyType(string? fullType)
    {
        if (string.IsNullOrWhiteSpace(fullType)) return null;
        var comma = fullType.IndexOf(',');
        var left = comma >= 0 ? fullType.Substring(0, comma) : fullType;
        var lastDot = left.LastIndexOf('.');
        return lastDot >= 0 ? left.Substring(lastDot + 1) : left;
    }
    
    // re-used existing DTO ??
    public record ProjectEntityDto(
        string ProjectId,
        string EntityType,
        string EntityId,
        string? ParentId,
        IDictionary<string, object> Components
    );

}
