using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private ThirdPersonCharacter character;

    public GameObject _faction;
    private List<GameObject> _cosasEncontradas = new List<GameObject>();
    private List<Vector2> _positionsToMove = new List<Vector2>();
    private bool _isMoving = false;
    private bool _isSearching = false;

    public event Action<GameObject> OnPersonFinishToMove;
    public event Action<List<GameObject>> OnPersonEnterFaction;
    private void Start()
    {
        
    }
    void Update()
    {
        if (agent.remainingDistance > agent.stoppingDistance)
        {  
            character.Move(agent.desiredVelocity, false, false);
        }
        else if(_isMoving)
        {
            character.Move(Vector3.zero, false, false);
            _isMoving = false;
            OnPersonFinishToMove?.Invoke(gameObject);
        }
        RunPositionsToMove();
    }

    //Return true when entity arrive at destination, retun false if is on the way
    public void MoveToPosition(Vector2 position)
    {
        _isMoving = true;
        agent.SetDestination(new Vector3(position.x,0,position.y));
    }
    public void AddPositionsToMove(List<Vector2> positions, bool repeat, bool isSearching)
    {
        _isSearching = isSearching ? true : false;
        _positionsToMove.Clear();
        foreach(Vector2 position in positions)
        {
            _positionsToMove.Add(position);
        }
    }
    private void RunPositionsToMove()
    {
        if (_isSearching && !_isMoving)
        {
            var positionFaction = new Vector2(_faction.transform.position.x, _faction.transform.position.z);
            var positionThis = new Vector2(transform.position.x, transform.position.z);
            _positionsToMove = _positionsToMove.OrderBy(x => Vector2.Distance(x, positionFaction) + (Vector2.Distance(x, positionThis) * 2)).ToList();
        }
        if (!_isMoving && _positionsToMove.Count > 0)
        {
            var positionToMove = _positionsToMove.First();
            //Debug.Log("Sended to " + positionToMove);
            MoveToPosition(positionToMove);
            _positionsToMove.Remove(positionToMove);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Resource")) && !_cosasEncontradas.Contains(other.gameObject))
        {
            //Debug.Log("Algo encontre" + other.gameObject.layer);
            _cosasEncontradas.Add(other.gameObject);
        }
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Faction")))
        {
            //Debug.Log("Volvi a base");
            OnPersonEnterFaction?.Invoke(_cosasEncontradas);
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
