window.TwinAppGraphics = (() => {
    let engine = null;
    let scene = null;

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

        engine.runRenderLoop(() => scene.render());
        window.addEventListener("resize", () => engine.resize());
    }

    function loadProject(projectId) {
        if (!scene) return;

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
        }

        // Optional: force a render update immediately
        if (engine) engine.requestAnimationFrame(() => scene.render());
    }

    function addAsset(assetId, path) { /* ... */ }
    function clearScene() { /* ... */ }

    return { initBabylon, loadProject, addAsset, clearScene };
})();
