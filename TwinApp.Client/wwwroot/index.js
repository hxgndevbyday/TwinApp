window.initBabylon = () => {
    const canvas = document.getElementById("renderCanvas");

    if (!canvas) {
        console.warn("Canvas element not found for Babylon initialization.");
        return;
    }

    // Create Babylon engine and scene
    const engine = new BABYLON.Engine(canvas, true);
    const scene = new BABYLON.Scene(engine);

    // Camera
    const camera = new BABYLON.ArcRotateCamera(
        "camera",
        Math.PI / 2,
        Math.PI / 4,
        6,
        BABYLON.Vector3.Zero(),
        scene
    );
    camera.attachControl(canvas, true);

    // Light
    const light = new BABYLON.HemisphericLight(
        "light",
        new BABYLON.Vector3(1, 1, 0),
        scene
    );

    // Example mesh (optional)
    const sphere = BABYLON.MeshBuilder.CreateSphere(
        "sphere",
        { diameter: 2 },
        scene
    );

    // Render loop
    engine.runRenderLoop(() => {
        scene.render();
    });

    // Resize handling
    window.addEventListener("resize", () => {
        engine.resize();
    });

    // Save references globally (optional)
    window.babylonEngine = engine;
    window.babylonScene = scene;

    console.log("Babylon initialized successfully.");
};

// Optional: helper to load a project
window.loadProject = (projectId) => {
    console.log(`Loading project: ${projectId}`);
    // Here you could add mock meshes or load glTF models for the project
};

// Optional: helper to add assets
window.addAsset = (assetId, modelPath) => {
    console.log(`Adding asset: ${assetId} from ${modelPath}`);
    // Implement asset addition here
};

// Optional: clear scene
window.clearScene = () => {
    if (window.babylonScene) {
        window.babylonScene.meshes.forEach(mesh => {
            if (mesh.name !== "__root__") mesh.dispose();
        });
        console.log("Scene cleared");
    }
};

window.loadProject = (projectId) => {
    console.log(`Loading project: ${projectId}`);

    if (!window.babylonScene) return;

    // Clear existing meshes
    window.babylonScene.meshes.forEach(m => {
        if (m.name !== "__root__") m.dispose();
    });

    // Mock CAD objects based on project ID
    const createMockCAD = (name, position) => {
        const box = BABYLON.MeshBuilder.CreateBox(name, { size: 1 }, window.babylonScene);
        box.position = position;
        return box;
    };

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
};
