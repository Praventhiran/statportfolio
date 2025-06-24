import { MindARThree } from './libs/mindar-image-three.prod.js';
import { GLTFLoader } from './libs/GLTFLoader.js';
import * as THREE from './libs/three.min.js';
import * as TWEEN from './libs/tween.umd.js';

document.addEventListener("DOMContentLoaded", async () => {
  const mindarThree = new MindARThree({
    container: document.querySelector("#ar-container"),
    imageTargetSrc: "./assets/targets.mind",
  });

  const { renderer, scene, camera } = mindarThree;
  const anchor = mindarThree.addAnchor(0);
  const textureLoader = new THREE.TextureLoader();

  // === Card background ===
  const cardTex = await textureLoader.loadAsync("./assets/card-art.png");
  const cardMat = new THREE.MeshBasicMaterial({ map: cardTex, transparent: true });
  const cardGeo = new THREE.PlaneGeometry(1, 1.4);
  const cardMesh = new THREE.Mesh(cardGeo, cardMat);
  cardMesh.rotation.x = -Math.PI / 2;
  cardMesh.position.y = 0.01;
  cardMesh.scale.set(0, 0, 0);
  anchor.group.add(cardMesh);

  // === Stats panel ===
  const statsTex = await textureLoader.loadAsync("./assets/stats-panel.png");
  const statsMat = new THREE.MeshBasicMaterial({ map: statsTex, transparent: true });
  const statsGeo = new THREE.PlaneGeometry(0.8, 0.4);
  const statsMesh = new THREE.Mesh(statsGeo, statsMat);
  statsMesh.position.set(0, 1.3, 0);
  statsMesh.scale.set(0, 0, 0);
  anchor.group.add(statsMesh);

  // === 3D character model ===
  const gltfLoader = new GLTFLoader();
  const model = await gltfLoader.loadAsync('./assets/character.glb');
  const character = model.scene;
  character.scale.set(0, 0, 0);
  character.position.set(0, 0, 0);
  anchor.group.add(character);

  // === Start camera and AR loop ===
  await mindarThree.start();

  renderer.setAnimationLoop(() => {
    renderer.render(scene, camera);
    TWEEN.update();
  });

  // === Show everything when image target is found ===
  anchor.onTargetFound = () => {
    new TWEEN.Tween(character.scale).to({ x: 1, y: 1, z: 1 }, 800)
      .easing(TWEEN.Easing.Back.Out).start();

    new TWEEN.Tween(cardMesh.scale).to({ x: 1, y: 1, z: 1 }, 1000)
      .easing(TWEEN.Easing.Quadratic.Out).delay(300).start();

    new TWEEN.Tween(statsMesh.scale).to({ x: 1, y: 1, z: 1 }, 800)
      .easing(TWEEN.Easing.Elastic.Out).delay(600).start();
  };

  // === Hide everything when target is lost ===
  anchor.onTargetLost = () => {
    character.scale.set(0, 0, 0);
    cardMesh.scale.set(0, 0, 0);
    statsMesh.scale.set(0, 0, 0);
  };

  // === Optional Scan Again button ===
  const scanBtn = document.getElementById("scan-again-btn");
  if (scanBtn) {
    scanBtn.addEventListener("click", async () => {
      await mindarThree.stop();
      await mindarThree.start();
    });
  }
});
