
THREE.GLTFLoader = function ( manager ) {
	this.manager = ( manager !== undefined ) ? manager : THREE.DefaultLoadingManager;
};
THREE.GLTFLoader.prototype = {
	load: function ( url, onLoad, onProgress, onError ) {
		var loader = new THREE.FileLoader( this.manager );
		loader.setResponseType( 'arraybuffer' );
		loader.load( url, function ( data ) {
			try {
				onLoad( new THREE.GLTFLoader().parse( data ) );
			} catch ( e ) {
				if ( onError ) onError( e );
			}
		}, onProgress, onError );
	},
	parse: function ( data ) {
		const loader = new THREE.GLTFLoader();
		return loader.parse( data );
	}
};
