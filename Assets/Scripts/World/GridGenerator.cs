using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject hexTilePrefab;

    [SerializeField] int mapWidth = 30;
    [SerializeField] int mapHeight = 30;

    float tileXOffset = 1.8f;
    float tileZOffset = 1.565f;

    GameObject[] tileList;
    //List<GameObject> explosedTileList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        tileList = new GameObject[mapWidth * mapHeight];
        CreateGridMap();
        NetworkManager.instance.gridGenerator = this;
    }

    // Update is called once per frame
    void CreateGridMap()
    {
        int id = 0;

        float mapXMin = -mapWidth / 2;
        float mapXMax = mapWidth / 2;

        float mapZMin = -mapHeight / 2;
        float mapZMax = mapHeight / 2;

        for (float x = mapXMin; x < mapXMax; x++)
        {
            for (float z = mapZMin; z < mapZMax; z++)
            {
                GameObject tileGameObject = Instantiate(hexTilePrefab);
                Vector3 pos;

                if (z % 2 == 0)
                {
                    pos = new Vector3(x * tileXOffset, 0, z * tileZOffset);
                }
                else
                {
                    pos = new Vector3(x * tileXOffset + tileXOffset / 2, 0, z * tileZOffset);
                }
                SetTileInfo(tileGameObject, x, z, pos, id);

                tileList[id] = tileGameObject;
                id++;
            }
        }
    }

    void SetTileInfo(GameObject tileGameObject, float x, float z, Vector3 pos, int id)
    {
        tileGameObject.transform.parent = transform;
        tileGameObject.name = x.ToString() + ", " + z.ToString();
        tileGameObject.transform.position = pos;
        tileGameObject.GetComponent<Tile>().Initialize(id, this);
    }

    public void ResetWorld()
    {
        for (int i=0; i< mapWidth * mapHeight; i++)
        {
            //tileList[i].gameObject.SetActive(true);
            tileList[i].GetComponent<Tile>().RecoverTile();
        }
        ServerSend.ResetWorld();
    }

    /*
     * In order to get grid with round shape
    private void OnTriggerExit(Collider other)
    {
        Destroy(other.gameObject);
    }
    */
}
