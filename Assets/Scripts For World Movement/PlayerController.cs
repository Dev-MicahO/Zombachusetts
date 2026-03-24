using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class PlayerController : MonoBehaviour
{
    
    private bool isMoving = false;
    [SerializeField] float moveSpeed = 5f;
    private Rigidbody2D rb;
    private float randValue;
    private float amountSinceLastFight = 0f;
    private bool isEncounterLoading = false;

    //tilemap that the player walks on
    [SerializeField] Tilemap groundTileMap;

    //tilemap that the player collides with
    [SerializeField] Tilemap collisionTilemap;

    //controls that use Unity's input sysetem
    private PlayerWorldMovement controls;
    
    private void Awake() 
    {
        controls = new PlayerWorldMovement();
    }

    private void OnEnable()
    {
        controls.Enable();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //we read in the vector value from the inputSystem
        controls.World.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
    }

    private void TriggerRandomEncounter()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogError("GameSession instance not found.");
            return;
        }

        if (SceneChanger.Instance == null)
        {
            Debug.LogError("SceneChanger instance not found.");
            return;
        }

        isEncounterLoading = true;

        GameSession.Instance.isRandomEncounter = true;
        GameSession.Instance.returnPlayerPosition = transform.position;
        GameSession.Instance.hasReturnPosition = true;

        SceneChanger.Instance.LoadScene("Battlescene");
    }

    private void Move(Vector2 direction) 
    {
        //actually move the player in a direction by transforming  or MovePosition to move the player into position
        if(CanMove(direction) && !isMoving)
        {
            //transform.position += (Vector3)direction;
            Vector2 targetPosition = rb.position + direction;
            //use a IEnumerator for some smooth movement
            StartCoroutine(SmoothMovement(targetPosition));
            
            //we will have a random encounter here in the movement for now. 
            //Later it could be added to a overhead game manager
            float random = Random.value;
            randValue = random + amountSinceLastFight;

            if(randValue > .99f && !isEncounterLoading)
            {
                Debug.Log("Trigger fight call");
                amountSinceLastFight = 0f;
                TriggerRandomEncounter();
            }
            else
            {
                //chance of getting a encounter increases with every step
                //a encounter is guaranteeded eventually
                amountSinceLastFight = amountSinceLastFight + 0.01f;
                Debug.Log("No fight this time");
            }


        }
    }

    private IEnumerator SmoothMovement(Vector2 targetPosition)
    {
        //while were moving we shouldn't allow you to move again
        isMoving = true;

        //if were still far away from the location then continue moving towards destination
        while ((targetPosition - rb.position).sqrMagnitude > 0.001f)
        {
            //move towards final destination slowly
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
            //set up position on the way to final position and move towards it
            rb.MovePosition(newPos);
            //wait for physics to update stuff
            yield return new WaitForFixedUpdate();
        }

        //move to the final position to ensure you can't drift
        rb.MovePosition(targetPosition); 
        isMoving = false;
    }

    private bool CanMove(Vector2 direction)
    {
        //get the grid position of the player
        Vector3Int gridPosition = groundTileMap.WorldToCell(transform.position + (Vector3)direction);

        //if their is no tile or a tile from the top grid player will be unable to move to it
        if(!groundTileMap.HasTile(gridPosition) || collisionTilemap.HasTile(gridPosition))
        {
            return false;
        }
        return true;
    }
}
