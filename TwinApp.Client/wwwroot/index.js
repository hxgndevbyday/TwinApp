// TwinApp.Client/main.js

window.initBabylon = () => {
    TwinAppGraphics.init("renderCanvas");
};

window.loadProject = (projectId) => {
    TwinAppGraphics.loadProject(projectId);
};

window.addAsset = (assetId, modelPath) => {
    TwinAppGraphics.addAsset(assetId, modelPath);
};

window.clearScene = () => {
    TwinAppGraphics.clearScene();
};



