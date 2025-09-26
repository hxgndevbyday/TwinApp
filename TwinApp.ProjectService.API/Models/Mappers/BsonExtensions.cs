namespace TwinApp.ProjectService.API.Models.Mappers;

using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

public static class BsonExtensions
{
    public static Dictionary<string, object?> ToDictionaryRecursive(this BsonDocument doc)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var el in doc.Elements)
        {
            var v = el.Value;

            if (v.IsBsonDocument)
            {
                dict[el.Name] = v.AsBsonDocument.ToDictionaryRecursive();
            }
            else if (v.IsBsonArray)
            {
                var arr = v.AsBsonArray;

                // Case 1: array of primitives → keep as list
                if (arr.All(a => !a.IsBsonDocument))
                {
                    dict[el.Name] = arr.Select(ConvertBsonValue).ToList();
                }
                else
                {
                    // Case 2: array of documents → recurse
                    var children = new List<Dictionary<string, object?>>();

                    foreach (var child in arr)
                    {
                        if (child.IsBsonDocument)
                        {
                            children.Add(child.AsBsonDocument.ToDictionaryRecursive());
                        }
                        else
                        {
                            children.Add(new Dictionary<string, object?>
                            {
                                { "Value", ConvertBsonValue(child) }
                            });
                        }
                    }

                    dict[el.Name] = children;
                }
            }
            else
            {
                dict[el.Name] = ConvertBsonValue(v);
            }
        }

        return dict;
    }

    private static object? ConvertBsonValue(BsonValue value)
    {
        if (value.IsBsonNull) return null;
        if (value.IsString) return value.AsString;
        if (value.IsInt32) return value.AsInt32;
        if (value.IsInt64) return value.AsInt64;
        if (value.IsDouble) return value.AsDouble;
        if (value.IsBoolean) return value.AsBoolean;
        if (value.IsValidDateTime) return value.ToUniversalTime();

        // fallback: raw string
        return value.ToString();
    }
}
