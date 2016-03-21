/*
THIS SCRIPT IS CURRENTLY NOT USED AT ALL BUT I'LL KEEP IT HERE BECAUSE IT'S NICE TO HAVE

using UnityEngine;
using System.Collections;

public class DetectTerrain : MonoBehaviour {
	int surfaceIndex = 0;

	private Terrain terrain;
	private TerrainData terrainData;
	private Vector3 terrainPos;

	function Start()
	{
	    terrain = Terrain.activeTerrain;
	    terrainData = terrain.terrainData;
	    terrainPos = terrain.transform.position;
	}

	function Update()
	{
	    surfaceIndex = GetMainTexture( transform.position );
	    //Debug.Log(surfaceIndex);
	}

	// - just for GUI demonstration -
	function OnGUI()
	{
	    GUI.Box( Rect( 10, 10, 25, 25 ), surfaceIndex.ToString() );
	}

	// ----

	public float[] GetTextureMix(Vector3 worldPos )
	{
	    // returns an array containing the relative mix of textures
	    // on the main terrain at this world position.

	    // The number of values in the array will equal the number
	    // of textures added to the terrain.

	    // calculate which splat map cell the worldPos falls within (ignoring y)
	    var mapX : int = parseInt( ((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth );
	    var mapZ : int = parseInt( ((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight );

	    // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
	    var splatmapData : float[,,] = terrainData.GetAlphamaps( mapX, mapZ, 1, 1 );

	    // extract the 3D array data to a 1D array:
	    var cellMix : float[] = new float[ splatmapData.GetUpperBound(2) + 1 ];

	    for ( var n : int = 0; n < cellMix.Length; n ++ )
	    {
	       cellMix[n] = splatmapData[ 0, 0, n ];
	    }

	    return cellMix;
	}

	function GetMainTexture( worldPos : Vector3 ) : int
	{
	    // returns the zero-based index of the most dominant texture
	    // on the main terrain at this world position.
	    var mix : float[] = GetTextureMix( worldPos );

	    var maxMix : float = 0;
	    var maxIndex : int = 0;

	    // loop through each mix value and find the maximum
	    for ( var n : int = 0; n < mix.Length; n ++ )
	    {
	       if ( mix[n] > maxMix )
	       {
	         maxIndex = n;
	         maxMix = mix[n];
	       }
	    }

	    return maxIndex;
	}
}*/
