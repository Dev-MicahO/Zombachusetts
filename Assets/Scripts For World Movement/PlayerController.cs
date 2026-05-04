using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    private Vector2 facingDirection;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite facingUp;
    [SerializeField] private Sprite facingDown;
    [SerializeField] private Sprite facingLeft;
    [SerializeField] private Sprite facingRight;
    
    private Vector2 movingInput;
    private bool isMoving = false;
    [SerializeField] float moveSpeed = 5f;
    private Rigidbody2D rb;
    private float randValue;
    private float amountSinceLastFight = 0f;
    private int stepsSinceLastEncounter = 0;
    [SerializeField] float encounterBufferSteps = 5f; // Minimum steps before an encounter can occur
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

    controls.World.Movement.performed += ctx => movingInput = ctx.ReadValue<Vector2>();
    controls.World.Movement.canceled += ctx => movingInput = Vector2.zero;
    controls.World.Interact.performed += ctx => Interact();
}

    private void OnDisable()
    {
        controls.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
    {
        UpdateFacingDirection(Vector2.down);
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

        // Save the exact overworld scene we came from.
        // This fixes battle return after loading a save file.
        string currentSceneName = SceneManager.GetActiveScene().name;
        GameSession.Instance.randomEncounterReturnScene = currentSceneName;
        GameSession.Instance.currentOverworldScene = currentSceneName;

        // Send player to loading screen first, then Loading scene sends them to battle.
        GameSession.Instance.loadingTargetScene = "Battlescene";
        GameSession.Instance.loadingReturnToPreviousScene = false;
        GameSession.Instance.loadingScreenDuration = 0.25f;

        SceneChanger.Instance.LoadScene("Loading");
    }
    private void Update()
    {   
        
        if (!isMoving)
        {
            
            Vector2 currentInput = controls.World.Movement.ReadValue<Vector2>();

            if (currentInput != Vector2.zero)
            {
                Vector2 direction = GetCardinalDirection(currentInput);
                Move(direction);
            }
        }
    }

    private void Interact()
    {
        Debug.Log("E PRESSED");
        Vector3 positionInFrontOfPlayer = transform.position + (Vector3)facingDirection;
        //Vector3Int frontTile = groundTileMap.WorldToCell(positionInFrontOfPlayer);

        Collider2D hit = Physics2D.OverlapPoint(positionInFrontOfPlayer);

        if(hit != null)
        {
            IInteractable interactable = hit.GetComponent(typeof(IInteractable)) as IInteractable;

            if(interactable != null && interactable.CanInteract())
            {
                interactable.Interact();
                return;
            }
        }
        

    }

    private void Move(Vector2 direction) 
    {
        UpdateFacingDirection(direction);
        
        //actually move the player in a direction by transforming  or MovePosition to move the player into position
        if(CanMove(direction) && !isMoving)
        {
            //transform.position += (Vector3)direction;
            Vector2 targetPosition = rb.position + direction;
            //use a IEnumerator for some smooth movement
            StartCoroutine(SmoothMovement(targetPosition));
            // Increase the step count
            stepsSinceLastEncounter++;
            
            // Safe zones disable random encounters
            if (GameSession.Instance != null && GameSession.Instance.IsInSafeArea())
            {
                amountSinceLastFight = 0f;
                stepsSinceLastEncounter = 0;
                return;
            }
            
            //we will have a random encounter here in the movement for now. 
            //Later it could be added to a overhead game manager
            float random = Random.value;
            randValue = random + amountSinceLastFight;

            if(randValue > .99f && !isEncounterLoading && stepsSinceLastEncounter >= encounterBufferSteps)
            {
                Debug.Log("Trigger fight call");
                amountSinceLastFight = 0f;
                stepsSinceLastEncounter = 0;
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
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime * 0.6f);
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

    private Vector2 GetCardinalDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return new Vector2(Mathf.Sign(input.x), 0);
        }
        else
        {
            return new Vector2(0, Mathf.Sign(input.y));
        }
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        facingDirection = direction;
        
        if(direction == Vector2.up)
        {
            spriteRenderer.sprite = facingUp;   
        }
        else if(direction == Vector2.down)
        {
            spriteRenderer.sprite = facingDown;
        }
        else if(direction == Vector2.left)
        {
            spriteRenderer.sprite = facingLeft;
        }
        else if(direction == Vector2.right)
        {
            spriteRenderer.sprite = facingRight;
        }
    }
}
