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

export function loadBabylon() {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = '_content/TwinApp.Client.Graphics/babylonGraphics.js';
        script.onload = resolve;
        script.onerror = reject;
        document.head.appendChild(script);
    });
}

