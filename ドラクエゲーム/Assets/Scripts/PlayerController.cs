using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region  "Player Attributes"
    [SerializeField] Animator playerAnimator;
    [SerializeField] Transform playerHead;
    [SerializeField] Transform playerNeck;
    [SerializeField] Transform lookingAt;
    [SerializeField] Vector3 relativePosition;
    [SerializeField] Transform cameraAnchor;
    [SerializeField] bool lockedOnTarget;
    [SerializeField] float speed;
    [SerializeField, Range(0, 5)] float walkSpeed;
    [SerializeField, Range(5, 10)] float runSpeed;
    [SerializeField, Range(0, 100)] float rotationSpeed;
    [SerializeField, Range(0, 1)] float speedSmoothTime;
    [SerializeField, Range(0, 1)] float turnSmoothTime;
    private float speedSmoothVelocity;
    private float turnSmoothVelocity;
    [SerializeField, Range(0, 1)] float jumpSpeed;
    [SerializeField, Range(0, 1)] float jump;
    [SerializeField] bool grounded;
    [SerializeField] bool walled;
    [SerializeField] bool running;
    [SerializeField] CameraController camController;

    private int bufferedLoadout;
    float timeCount;


    #region "Animation Attributes"
    [SerializeField, Range(-1, 1)] float vertical;
    [SerializeField, Range(-1, 1)] float horizontal;

    /***
    0 = unarmed
    1 = one handed sword
    2 = two handed sword
    3 = sword and shield
    4 = bow
    ***/
    [SerializeField, Range(0, 4)] int loadout;
    #endregion


    #endregion
    // Start is called before the first frame update
    void Start()
    {
        playerAnimator.SetInteger("Loadout", loadout);
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        #region "Target Lock-On"
        //when the player clicks the mouse wheel, it'll check for whether or not there's an enemy in sight
        if (Input.GetMouseButtonDown(2))
        {
            if (!lockedOnTarget)
            {
                RaycastHit hit;
                Vector3 fwd = playerHead.transform.TransformDirection(Vector3.forward);
                if (Physics.Raycast(playerHead.transform.position, fwd, out hit, 10f))
                {
                    //if there's a target, the player will look at it
                    if (hit.collider.CompareTag("Target"))
                    {
                        //何だか。。。なんか違うと思う
                        //Debug.Log("There is something in front of the object!");
                        Debug.DrawRay(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                        lockedOnTarget = true;
                        playerAnimator.SetBool("Locked", lockedOnTarget);
                        lookingAt = hit.collider.transform;
                    }
                    else
                    {
                        //Debug.Log("No target in sight.");
                        Debug.DrawRay(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                    }
                }
                else
                {
                    Debug.DrawRay(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                }
            }
            else
            {
                lookingAt = null;
                camController.target = this.transform;
                lockedOnTarget = false;
                playerAnimator.SetBool("Locked", lockedOnTarget);
            }
        }

        if (lookingAt)
        {
            ///*
            relativePosition = lookingAt.position - playerHead.position;
            
            //playerNeck.rotation = Quaternion.Slerp(playerNeck.rotation, Quaternion.LookRotation(relativePosition) * Quaternion.Euler(-playerHead.localEulerAngles), Time.deltaTime * rotationSpeed);
            //Debug.Log(-playerHead.localRotation.eulerAngles);
            //cameraAnchor.rotation = Quaternion.RotateTowards(cameraAnchor.rotation, Quaternion.LookRotation(relativePosition), Time.deltaTime * rotationSpeed);
            //playerHead.transform.rotation = Quaternion.RotateTowards(playerHead.transform.rotation, Quaternion.LookRotation(relativePosition), Time.time * rotationSpeed);
            Debug.DrawRay(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * Vector3.Distance(playerHead.transform.position, lookingAt.transform.position), Color.red);
            //*/
        }

        #endregion

        #region "Unlocked Movement"
        else
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 inputDir = input.normalized;
            if (inputDir != Vector2.zero)
            {
                float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
            }

            running = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
            speed = Mathf.SmoothDamp(speed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

            transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);

            float animationSpeedPercent = ((running) ? 1 : .5f) * inputDir.magnitude;
            playerAnimator.SetFloat("Vertical", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
            playerAnimator.SetLookAtWeight(0f);
        }
        //looks at the object for as long as it doesn't look unnatural 

        //whenever it's not looking at anything the player's head will just look ahead

        #endregion

        //sets animator values in directional speed (horizontal and vertical)


        #region "Animation Control"


        if (loadout != bufferedLoadout)
        {
            bufferedLoadout = loadout;
            playerAnimator.SetInteger("Loadout", loadout);
        }

        /*
        Changing loadouts:
        R = disarm
        E = next
        Q = previous
        */
        if (Input.GetKeyDown(KeyCode.R))
        {
            loadout = 0;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (loadout < 4)
            {
                loadout++;
            }
            else
            {
                loadout = 0;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (loadout > 0)
            {
                loadout--;
            }
            else
            {
                loadout = 4;
            }
        }
        /*
        playerAnimator.SetFloat("Vertical", vertical);
        playerAnimator.SetFloat("Horizontal", horizontal);
        playerAnimator.SetFloat("Jump", jump);
        */

        #endregion

        //camera rotation is controlled by mouse movement, adding to its rotation around the camera anchor
        #region "Mouse Camera Control"
        cameraAnchor.Rotate(Input.GetAxis("Mouse Y") / 2, Input.GetAxis("Mouse X") / 2, 0);
        #endregion


    }
    void OnAnimatorIK()
    {
        if (playerAnimator)
        {
            //Debug.Log(playerHead.transform.rotation.y);
            if (lookingAt)
            {
                ///*
                //relativePosition = lookingAt.position - playerHead.position;
                //playerNeck.rotation = Quaternion.Slerp(playerNeck.rotation, Quaternion.LookRotation(relativePosition) * Quaternion.Euler(-playerHead.localEulerAngles), Time.deltaTime * rotationSpeed);
                //Debug.Log(-playerHead.localRotation.eulerAngles);
                //cameraAnchor.rotation = Quaternion.RotateTowards(cameraAnchor.rotation, playerHead.rotation, Time.deltaTime * rotationSpeed);
                //playerHead.transform.rotation = Quaternion.RotateTowards(playerHead.transform.rotation, Quaternion.LookRotation(relativePosition), Time.time * rotationSpeed);
                //Debug.DrawRay(playerHead.transform.position, playerHead.transform.TransformDirection(Vector3.forward) * Vector3.Distance(playerHead.transform.position, lookingAt.transform.position), Color.red);
                //*/
                //Debug.Log(playerHead.eulerAngles);
                if (playerHead.localEulerAngles.y < 70f || playerHead.localEulerAngles.y > 290)
                {

                    playerAnimator.SetLookAtWeight(weight: 1f, bodyWeight: 0.5f, headWeight: 1.0f, eyesWeight: 0, clampWeight: 0.9f);
                    playerAnimator.SetLookAtPosition(lookingAt.position);

                }
            }
            if (!lookingAt)
            {
                //Debug.Log(playerHead.transform.rotation);
                //これはまだ動かないんです
                ///*
                //cameraAnchor.rotation = Quaternion.RotateTowards(cameraAnchor.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * rotationSpeed);
                //playerNeck.rotation = Quaternion.RotateTowards(playerNeck.transform.rotation * Quaternion.Inverse(playerHead.rotation), Quaternion.Euler(0, 0, 0), Time.deltaTime * rotationSpeed);
                //*/
                playerAnimator.SetLookAtWeight(0);
            }
        }
    }
}
