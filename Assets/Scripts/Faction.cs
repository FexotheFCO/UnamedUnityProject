using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction : MonoBehaviour
{
    [SerializeField] private GameObject SpawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject Faro;

    List<GameObject> People;
    
    private List<Vector2> _knowPlaces = new List<Vector2>();
    private List<Vector2> _unknowPlaces = new List<Vector2>();
    private List<GameObject> _resources = new List<GameObject>();

    private void Awake()
    {
        People = new List<GameObject>();
        for (int i = 0; i < 1; i++)
        {
            GameObject people = Instantiate(playerPrefab, SpawnPoint.transform.position, Quaternion.identity);
            people.GetComponent<PlayerController>()._faction = gameObject;
            People.Add(people);
        }
        for (int x = -5; x < 5; x++)
        {
            for (int y = -5; y < 5; y++)
            {
                _unknowPlaces.Add(new Vector2(x * 5, y * 5));
            }
        }
    }
    private void Start()
    {
        Search();
    }

    private void Update()
    {
    }

    private void Search()
    {
        //NECESITAMOS UNA MANERA DE CONFIRMAR CUANDO LLEGA A LA POSICION QUE LE MANDAS
        var person = People.First();
        var playerController = person.GetComponent<PlayerController>();
        playerController.OnPersonEnterFaction += ReceiveNewResources;
        //MoveToPosition(person,(Vector2)SearchClosestPosition());
        playerController.AddPositionsToMove(_unknowPlaces, false, true);
    }
    private Vector2 SearchClosestPosition()
    {
        Vector2 closestPosition = Vector2.one * 10;
        foreach (Vector2 position in _unknowPlaces)
        {
            float dist = Vector2.Distance(position, transform.position);
            if (position.magnitude < closestPosition.magnitude)
                closestPosition = position;
        }
        return closestPosition;
    }
    private void MoveToRandomPosition(GameObject person)
    {
        var playerController = person.GetComponent<PlayerController>();
        Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-50, 50), 0, UnityEngine.Random.Range(-50, 50));
        Debug.Log(randomPos);
        playerController.MoveToPosition(randomPos);
    }
    private void MoveToPosition(GameObject person, Vector3 position)
    {
        var playerController = person.GetComponent<PlayerController>();
        playerController.MoveToPosition(position);
    }

    private void ReceiveNewResources(List<GameObject> NewResources)
    {
        foreach(GameObject resource in NewResources)
        {
            Debug.Log("Traigo shit" + resource);
            if (!_resources.Contains(resource))
            {
                _resources.Add(resource);
            }
        }
    }
}

    /*ROTAR ALGO
        Faro.transform.Rotate(0, .5f, 0, Space.Self);
        
        RaycastHit hit;
        Vector3 dir = Faro.transform.forward;
        dir.y = Faro.transform.position.y;
        dir.x *= 100;
        dir.z *= 100;
        Debug.DrawLine(Faro.transform.position, dir, Color.green);
        */
    /*
     * TIRAR RAY CAST AL REDEDOR DE UN OBJETO, HACE UNAS COSAS FLASHERAS CON EL COSENO Y EL SENO MUY PIOLAS
    int RaysToShoot = 30;
    float angle = 0;
    for (int i = 0; i < RaysToShoot; i++)
    {
        float x = Mathf.Sin(angle);
        float y = Mathf.Cos(angle);
        angle += 2 * Mathf.PI / RaysToShoot;

        Vector3 dir = new Vector3(transform.position.x + x, 0, transform.position.z + y);
        dir *= 1000f;
        RaycastHit hit;
        Debug.DrawLine(transform.position, dir, Color.red);
        if (Physics.Raycast(transform.position, dir, out hit))
        {
            //here is how to do your cool stuff ;)
        }

    }*/

