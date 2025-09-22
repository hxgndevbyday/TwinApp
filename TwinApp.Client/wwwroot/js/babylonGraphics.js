window.TwinAppGraphics = (() => {
    let engine = null;
    let scene = null;
    let initializedResolve;
    let initialized = new Promise(resolve => { initializedResolve = resolve; });

    function initBabylon(canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.warn("Canvas not found, retrying...");
            setTimeout(() => initBabylon(canvasId), 50);
            return;
        }

        engine = new BABYLON.Engine(canvas, true);
        scene = new BABYLON.Scene(engine);

        const camera = new BABYLON.ArcRotateCamera(
            "camera", Math.PI / 2, Math.PI / 4, 6, BABYLON.Vector3.Zero(), scene
        );
        camera.attachControl(canvas, true);

        new BABYLON.HemisphericLight("light", new BABYLON.Vector3(1, 1, 0), scene);
        BABYLON.MeshBuilder.CreateSphere("sphere", { diameter: 2 }, scene);

        engine.runRenderLoop(() => {
            if (scene) scene.render();
        });

        window.addEventListener("resize", () => {
            if (engine) engine.resize();
        });

        
        // Signal that Babylon is ready
        initializedResolve();
    }

    async function loadProject(projectId) {
        // Wait for engine/scene to be ready
        await initialized;

        if (!scene) {
            console.warn("Babylon scene not ready yet, cannot load project.");
            return;
        }

        // Remove old meshes except root
        scene.meshes.forEach(mesh => {
            if (mesh.name !== "__root__") mesh.dispose();
        });

        // Helper to create mock CAD objects
        const createMockCAD = (name, position) => {
            const mesh = BABYLON.MeshBuilder.CreateBox(name, { size: 1 }, scene);
            mesh.position = position;
            return mesh;
        };

        // Example mock projects
        switch (projectId) {
            case "1": // Factory A
                createMockCAD("Machine1", new BABYLON.Vector3(0, 0.5, 0));
                createMockCAD("Machine2", new BABYLON.Vector3(2, 0.5, 0));
                break;
            case "2": // Warehouse B
                createMockCAD("Shelf1", new BABYLON.Vector3(-1, 0.5, 1));
                createMockCAD("Shelf2", new BABYLON.Vector3(1, 0.5, 1));
                break;
            case "3": // Plant C
                createMockCAD("Pump1", new BABYLON.Vector3(0, 0.5, -2));
                createMockCAD("Pump2", new BABYLON.Vector3(2, 0.5, -2));
                break;
            default:
                console.warn("Unknown project ID: " + projectId);
                loadCAD("sample-data/m-20ia_35m/m-20ia_35m.stl");
        }

        // Rendering is already handled by runRenderLoop
    }

    function addAsset(assetId, path) {
        // Optional: implement loading additional assets
    }

    function clearScene() {
        if (!scene) return;
        scene.meshes.forEach(mesh => {
            if (mesh.name !== "__root__") mesh.dispose();
        });
    }

    // Function to load STL model into the Babylon scene
    window.loadCAD = (filePath) => {
        if (!scene) {
            console.error("Scene not initialized yet.");
            return;
        }

        BABYLON.SceneLoader.ImportMesh(
            null,                // meshNames → null = load all
            "/",                 // rootUrl (relative to wwwroot in Blazor WASM)
            filePath,            // filename
            scene, // target scene
            (meshes) => {
                console.log("CAD loaded:", meshes);

                // Optional: scale + center the model
                meshes.forEach(m => {
                    m.scaling = new BABYLON.Vector3(0.01, 0.01, 0.01); // adjust as needed
                    m.position = BABYLON.Vector3.Zero();
                });
            },
            null,
            (scene, message, exception) => {
                console.error("Error loading CAD:", message, exception);
            }
        );
    };

    return { initBabylon, loadProject, addAsset, clearScene };
})();
