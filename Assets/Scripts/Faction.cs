using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction : MonoBehaviour
{
    [SerializeField] private GameObject SpawnPoint;
    [SerializeField] private GameObject playerPrefab;

    List<GameObject> People;
    
    //Lo que pienso sobre etas listas es que tal vez se pueda hacer un lista de objetos y dentro de esos objetos tener alguna
    //manera de decir si son know unknow o searching, lo mismo con las listas de recursos
    private List<Vector2> _knowPlaces = new List<Vector2>();
    private List<Vector2> _unknowPlaces = new List<Vector2>();
    private List<Vector2> _searchingPlaces = new List<Vector2>();

    private List<GameObject> _knowResources = new List<GameObject>();
    private List<GameObject> _exploitedResources = new List<GameObject>();

    private void Awake()
    {
        People = new List<GameObject>();
        //Crea personas
        for (int i = 0; i < 2; i++)
        {
            GameObject people = Instantiate(playerPrefab, SpawnPoint.transform.position, Quaternion.identity);
            people.GetComponent<PlayerController>()._faction = gameObject;
            People.Add(people);
        }
        //Crea matriz de posiciones no descubiertas
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
    }

    private void Update()
    {
        //AI
        if (People.Where(x => !x.GetComponent<PlayerController>().isAssigned).ToList().Count > 0)
        {
            if (_knowResources.Count > 0)
            {
                Debug.Log("Che tal vez es una buena idea ir a buscar esos recursos");
                ExploitResource(GetPersonUnasigned(), GetClosestUnxploitedResource());
            }
            else if (_knowResources.Count == 0)
            {
                Search(GetPersonUnasigned(), GetClosestUnknowPlaces(10));
                Debug.Log("Che tal vez es una buena explorar");
            }
        }
        
    }
    //MECANICAS
    private void ExploitResource(PlayerController person, GameObject resource)
    {
        person.OnPersonEnterFaction += ReceiveNewResourceDiscovered; //Aca deberia cambiar a otro metodo que reciva los recursos explotados
        person.OnPersonFinishAssign += PersonFinishAssign;

        _knowResources.Remove(resource);
        _exploitedResources.Add(resource);

        person.AddResourceToExploit(resource);
        person.isAssigned = true;
    }
    private void Search(PlayerController person, List<Vector2> positionsToMove)
    {
        //Tomo la primer persona que no este asignada a alguna tarea
        //Esta linea se podria hacer en un metodo ya que podria ser llamada varias veces en el codigo
        person.OnPersonEnterFaction += ReceiveNewResourceDiscovered;
        person.OnPersonFinishAssign += PersonFinishAssign;

        //cada posicion la saco de las que no se conocen y las pongo en las que se estan buscando
        foreach (Vector2 position in positionsToMove)
        {
            _unknowPlaces.Remove(position);
            _searchingPlaces.Add(position);
        }
        person.AddPositionsToMove(positionsToMove, false, true);
        person.isAssigned = true;
    }
    //SERVICES
    private PlayerController GetPersonUnasigned()
    {
        //Tomo la primer persona que no este asignada a alguna tarea
        return People.Where(x => !x.GetComponent<PlayerController>().isAssigned).First().GetComponent<PlayerController>();
    }
    private GameObject GetClosestUnxploitedResource()
    {
        //Tomo el recurso mas cercano a la base
        return _knowResources.OrderBy(x => Vector2.Distance(new Vector2(x.transform.position.x, x.transform.position.z), new Vector2(transform.position.x, transform.position.z))).First();
    }
    private List<Vector2> GetClosestUnknowPlaces(int cuantity)
    {
        //ordeno las posiciones no conocidas por distancia a la base, tomo la cantidad
        return _unknowPlaces.OrderBy(x => Vector2.Distance(x, new Vector2(gameObject.transform.position.x, gameObject.transform.position.z))).Take(cuantity).ToList();
    }
    //
    
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
    private void ReceiveNewResourceDiscovered(List<GameObject> NewResources)
    {
        foreach(GameObject resource in NewResources)
        {
            if (!_knowResources.Contains(resource))
            {
                _knowResources.Add(resource);
            }
        }
    }
    private void PersonFinishAssign(GameObject Person)
    {
        Person.GetComponent<PlayerController>().isAssigned = false;
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

