using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PersonController : MonoBehaviour
{

    // Start is called before the first frame update
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private ThirdPersonCharacter character;
    [NonSerialized]
    public bool isAssigned = false;
    [NonSerialized]
    public GameObject faction;
    
    public int cantidadDeRecursos = 0;


    private List<GameObject> _cosasEncontradas = new List<GameObject>();
    private List<Vector2> _positionsToMove = new List<Vector2>();
    private GameObject _resourceToExploit = null;

    const int _limiteDeRecursos = 50;
    private bool _isMoving = false;
    private bool _isSearching = false;
    private bool _isRepeating = false;
    private bool _stop = false;

    public event Action<GameObject> OnPersonFinishToMove;
    public event Action<List<GameObject>> OnPersonEnterFaction;//Aca en vez de pasarle una lista de objetos podriamos hacer una nueva clase con varias cosas para cuando llega, por ahora voy a seguir haciendo eventos
    //Incluso podria pasarle directamente el personController y ahi ir fijandome
    public event Action<GameObject> OnPersonFinishAssign;
    public event Action<PersonController> OnPersonEnterFactionWhitResources;
    void Update()
    {
        CheckRemainingDistance();
        if(_positionsToMove.Count > 0 && !_isMoving && !_stop)
            RunPositionsToMove();

    }
    public void AddResourceToExploit(GameObject resource)
    {
        //Agregar el camino que tiene que hacer y dsp programar la logica de explotacion de recursos
        _resourceToExploit = resource;
        List<Vector2> posiciones = new List<Vector2>()
        {
            new Vector2(faction.transform.position.x, faction.transform.position.z),
            new Vector2(_resourceToExploit.transform.position.x, _resourceToExploit.transform.position.z)
        };
        AddPositionsToMove(posiciones, true, false);
    }
    public void MoveToPosition(Vector2 position)
    {
        _isMoving = true;
        agent.SetDestination(new Vector3(position.x,0,position.y));
    }
    public void AddPositionsToMove(List<Vector2> positions, bool isPepeating, bool isSearching)
    {
        _isSearching = isSearching ? true : false;
        _isRepeating = isPepeating ? true : false;
        _positionsToMove.Clear();
        foreach(Vector2 position in positions)
        {
            _positionsToMove.Add(position);
        }
    }
    //Esto no solo chekea la distancia si no que mueve al character en la direccion adecuada, tal vez hay que cambiar el nombre
    private void CheckRemainingDistance()
    {
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            character.Move(agent.desiredVelocity, false, false);
        }
        else if (_isMoving)
        {
            character.Move(Vector3.zero, false, false);
            _isMoving = false;
            OnPersonFinishToMove?.Invoke(gameObject);
            if (_isSearching && _positionsToMove.Count == 0) 
            {
                //esto es un poco verga, porque no termina una asignacion termina de buscar, guarda con eso
                //Aca se supone que termina de buscar, tal vez estaria bueno hacer un evento especial para esto, por ahora lo manda a la base
                OnPersonFinishAssign?.Invoke(gameObject);
                MoveToPosition(new Vector2(faction.transform.position.x, faction.transform.position.z));
            }
        }
    }
    private void RunPositionsToMove()
    {
        if (_isSearching)
        {
            //Al estar en modo BUSQUEDA calculo de las proximas posiciones cual es la mejor, dependiendo de que tan lejos esten de la base y de la persona
            var positionFaction = new Vector2(faction.transform.position.x, faction.transform.position.z);
            var positionThis = new Vector2(transform.position.x, transform.position.z);
            //Aca se multiplica por 2 el calculo de la distancia respecto a la persona, ya que le quiero dar una prioridad a esa distancia
            _positionsToMove = _positionsToMove.OrderBy(x => Vector2.Distance(x, positionFaction) + (Vector2.Distance(x, positionThis) * 2)).ToList();
        }
        //Tomo la primer posicion en la lista, la remuevo y muevo a la persona
        var positionToMove = _positionsToMove.First();
        MoveToPosition(positionToMove);
        _positionsToMove.Remove(positionToMove);
        if (_isRepeating)
            _positionsToMove.Add(positionToMove);
    }
    private IEnumerator ExplotarRecurso()
    {
        _stop = true;
        while (cantidadDeRecursos < _limiteDeRecursos)
        {
            cantidadDeRecursos++;
            yield return new WaitForSeconds(1f);
            //Debug.Log("Agarre un recurso mas ahora tengo " + cantidadDeRecursos);
        }
        _stop = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Resource")))
        {
            if (!_cosasEncontradas.Contains(other.gameObject))
            {
                //Debug.Log("Algo encontre" + other.gameObject.layer);
                _cosasEncontradas.Add(other.gameObject);
            }else if (other.gameObject.Equals(_resourceToExploit))
            {
                Debug.Log("Buenas locuras aca estoy explotando un recurso");
                StartCoroutine("ExplotarRecurso");
            }   
        }
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Faction")))
        {
            //Debug.Log("Volvi a base");
            OnPersonEnterFaction?.Invoke(_cosasEncontradas);
            if (cantidadDeRecursos > 0)
                OnPersonEnterFactionWhitResources?.Invoke(this);
        }
    }
}
/*
 Camera cam = Camera.main;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveToPosition(new Vector3(10,0,10));
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }
 */
