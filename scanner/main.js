import { MindARThree } from './libs/mindar-image-three.prod.js';
import { GLTFLoader } from './libs/GLTFLoader.js';
import * as THREE from './libs/three.min.js';
import * as TWEEN from './libs/tween.umd.js';

const start = async () => {
  try {
    const mindarThree = new MindARThree({
      container: document.querySelector("#ar-container"),
      imageTargetSrc: "./assets/targets.mind",
      maxTrack: 1,
      filterMinCF: 0.0001,
      filterBeta: 0.001,
      warmupTolerance: 5
    });

    const { renderer, scene, camera } = mindarThree;
    const anchor = mindarThree.addAnchor(0);

    const textureLoader = new THREE.TextureLoader();

    // === 1. Digital Card Background ===
    const cardTex = textureLoader.load("./assets/card-art.png");
    const cardMat = new THREE.MeshBasicMaterial({ map: cardTex, transparent: true });
    const cardGeo = new THREE.PlaneGeometry(1, 1.4);
    const cardMesh = new THREE.Mesh(cardGeo, cardMat);
    cardMesh.rotation.x = -Math.PI / 2;
    cardMesh.position.y = 0.01;
    cardMesh.scale.set(0, 0, 0);
    anchor.group.add(cardMesh);

    // === 2. Stats Panel ===
    const statsTex = textureLoader.load("./assets/stats-panel.png");
    const statsMat = new THREE.MeshBasicMaterial({ map: statsTex, transparent: true });
    const statsGeo = new THREE.PlaneGeometry(0.8, 0.4);
    const statsMesh = new THREE.Mesh(statsGeo, statsMat);
    statsMesh.position.set(0, 1.3, 0);
    statsMesh.scale.set(0, 0, 0);
    anchor.group.add(statsMesh);

    // === 3. Load GLB Character ===
    const gltfLoader = new GLTFLoader();
    const model = await gltfLoader.loadAsync('./assets/character.glb');
    const character = model.scene;
    character.scale.set(0, 0, 0);
    character.position.set(0, 0, 0);
    anchor.group.add(character);

    // === 4. Start AR Engine ===
    console.log("Requesting camera access and starting MindAR...");
    await mindarThree.start();
    console.log("MindAR started successfully.");

    // === 5. Main render loop ===
    renderer.setAnimationLoop(() => {
      renderer.render(scene, camera);
      TWEEN.update();
    });

    // === 6. Show content on detection ===
    anchor.onTargetFound = () => {
      console.log("Target found!");

      new TWEEN.Tween(character.scale).to({ x: 1, y: 1, z: 1 }, 800)
        .easing(TWEEN.Easing.Back.Out).start();

      new TWEEN.Tween(cardMesh.scale).to({ x: 1, y: 1, z: 1 }, 1000)
        .easing(TWEEN.Easing.Quadratic.Out).delay(300).start();

      new TWEEN.Tween(statsMesh.scale).to({ x: 1, y: 1, z: 1 }, 800)
        .easing(TWEEN.Easing.Elastic.Out).delay(600).start();
    };

    // === 7. Hide content when target lost ===
    anchor.onTargetLost = () => {
      console.log("Target lost!");
      character.scale.set(0, 0, 0);
      cardMesh.scale.set(0, 0, 0);
      statsMesh.scale.set(0, 0, 0);
    };

  } catch (error) {
    console.error("❌ Error starting MindAR or accessing camera:", error);
    alert("Camera access failed or AR could not start. Please check permissions.");
  }
};

start();
