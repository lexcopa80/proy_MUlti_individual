// Variables globales del entorno 3D
let escena, camara, renderizador, reloj, mezcladorAnimacion, controles;
let animacionAccion; 

const audioCover = new Audio('vaca_lola.mp3');
audioCover.loop = true;

function initWebGL() {
    const contenedor = document.getElementById('canvas-container');
    if (!contenedor) return;
    
    reloj = new THREE.Clock();

    // 1. Crear Escena
    escena = new THREE.Scene();

    // 2. Configurar Cámara Inicial Base
    camara = new THREE.PerspectiveCamera(45, contenedor.clientWidth / contenedor.clientHeight, 0.1, 1000);
    camara.position.set(0, 2, 5); 

    // 3. Renderizador WebGL
    renderizador = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    renderizador.setSize(contenedor.clientWidth, contenedor.clientHeight);
    renderizador.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderizador.setClearColor(0x11141a, 1); 
    contenedor.appendChild(renderizador.domElement);

    // 4. Habilitar Controles de Vista 3D Orbital
    controles = new THREE.OrbitControls(camara, renderizador.domElement);
    controles.enableDamping = true; 
    controles.dampingFactor = 0.05;
    controles.maxDistance = 50;    
    controles.minDistance = 1;     

    // 5. Sistema de Iluminación Multi-Ángulo Exponencial
    const luzAmbiental = new THREE.AmbientLight(0xffffff, 1.2); 
    escena.add(luzAmbiental);

    const luzFrontal = new THREE.DirectionalLight(0xffffff, 1);
    luzFrontal.position.set(0, 4, 5); // <--- ERROR CORREGIDO AQUÍ
    escena.add(luzFrontal);

    const luzNeonVerde = new THREE.DirectionalLight(0x4cd137, 1.5); 
    luzNeonVerde.position.set(-5, 5, -5);
    escena.add(luzNeonVerde);

    // 6. Cargar el modelo .GLB
    const cargador = new THREE.GLTFLoader();
    
    cargador.load('modelo_fotogra.glb', function(gltf) {
        const personaje = gltf.scene;
        personaje.scale.set(0.3, 0.3, 0.3);
        escena.add(personaje);

        // --- ALGORITMO DE ENCUADRE Y ESCALA AUTOMÁTICA ---
        const cajaBusqueda = new THREE.Box3().setFromObject(personaje);
        const centroModelo = new THREE.Vector3();
        const tamanoModelo = new THREE.Vector3();
        
        cajaBusqueda.getCenter(centroModelo);
        cajaBusqueda.getSize(tamanoModelo);

        personaje.position.x += (personaje.position.x - centroModelo.x);
        personaje.position.y += (personaje.position.y - centroModelo.y);
        personaje.position.z += (personaje.position.z - centroModelo.z);

        const dimensionMaxima = Math.max(tamanoModelo.x, tamanoModelo.y, tamanoModelo.z);
        const distanciaCamara = dimensionMaxima / (2 * Math.tan(Math.PI * camara.fov / 360));
        
        // Multiplicador panorámico aumentado para alejar la cámara del modelo gigante
camara.position.set(0, tamanoModelo.y / 2, distanciaCamara * 28.5);
        
        const puntoObjetivo = new THREE.Vector3(0, tamanoModelo.y / 2, 0);
        camara.lookAt(puntoObjetivo);
        controles.target.copy(puntoObjetivo);

        console.log('Vista 3D Orbital configurada de forma interactiva.');

        // Control de Animaciones internas del GLB
        if (gltf.animations && gltf.animations.length > 0) {
            mezcladorAnimacion = new THREE.AnimationMixer(personaje);
            animacionAccion = mezcladorAnimacion.clipAction(gltf.animations[0]);
            animacionAccion.play();
            animacionAccion.paused = true; 
        }
    }, function(xhr) {
        console.log((xhr.loaded / xhr.total * 100) + '% cargado');
    }, function(error) {
        console.error('Error al procesar el modelo GLB:', error);
    });

    window.addEventListener('resize', onWindowResize, false);
    animate();
}

function onWindowResize() {
    const contenedor = document.getElementById('canvas-container');
    if (!contenedor) return;
    camara.aspect = contenedor.clientWidth / contenedor.clientHeight;
    camara.updateProjectionMatrix();
    renderizador.setSize(contenedor.clientWidth, contenedor.clientHeight);
}

function animate() {
    requestAnimationFrame(animate);
    
    const delta = reloj.getDelta();
    if (mezcladorAnimacion) {
        mezcladorAnimacion.update(delta);
    }

    if (controles) {
        controles.update();
    }

    if (renderizador && escena && camara) {
        renderizador.render(escena, camara);
    }
}

// CONTROLADOR DE EVENTO DEL BOTÓN INTERACTIVO
document.getElementById('btnIniciar').addEventListener('click', () => {
    if (audioCover.paused) {
        audioCover.play().catch(err => console.log("Permiso de audio requerido"));
        if (animacionAccion) {
            animacionAccion.paused = false;
        }
        document.getElementById('btnIniciar').innerText = "⏸️ PAUSAR INTERACTIVIDAD";
        document.getElementById('txtEstado').innerText = "Renderizando entorno WebGL interactivo en 3D orbital.";
    } else {
        audioCover.pause();
        if (animacionAccion) {
            animacionAccion.paused = true;
        }
        document.getElementById('btnIniciar').innerText = "🎵 INICIAR EXPERIENCIA MULTIMEDIA";
        document.getElementById('txtEstado').innerText = "Multimedia en pausa.";
    }
});

// Inicialización controlada
window.addEventListener('DOMContentLoaded', initWebGL);