using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterControl : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Insert Character Controller")]
    private CharacterController controller;

    [SerializeField] 
    [Tooltip("Insert Main Camera")]
    private Camera camera;
    
    [SerializeField] 
    [Tooltip("Insert Animator")]
    private Animator playerAnimator;
    
    [SerializeField] 
    [Tooltip("Insert Pokeball Prefab")]
    private GameObject pokeballPF;
    
    [SerializeField] 
    [Tooltip("Insert Pokeball Bone Transform")]
    private Transform pokeBallBone;
    
    public bool grounded;
    private Vector3 velocity;
    
    private float gravity = -9.8f;
    private float groundCastDist = 0.05f;

    public float runSpeed = 6f;
    public float speed = 2f;
    public float jumpHeight = 20f;

    private bool throwing = false;
    public float throwStrength = 4f;
    private GameObject instantiatedPokeball;
    
    
    // Start is called before the first frame update
    // void Start()
    // {
    //  
    // }

    // Update is called once per frame
    void Update()
    {
        //grab camera
        Transform playerTransform = transform;
        Transform cameraTransform = camera.transform;

        //Grounded
        grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCastDist);

        //Debug
        if (grounded)
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.blue);
        }
        else
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.red);
        }


        // Ground Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 movement = (playerTransform.right * x) + (playerTransform.forward * z);
        if (!throwing)
        {
            controller.Move(movement * (speed * Time.deltaTime));
            if (movement.magnitude > 0)
            {
                playerAnimator.SetBool("IsWalking", true);
            }
            else
            {
                playerAnimator.SetBool("IsWalking", false);
            }

            // ... (rest of the movement code)
        }
        else
        {
            // Set movement to zero when throwing
            controller.Move(Vector3.zero);
            playerAnimator.SetBool("IsWalking", false);
        }


        //stop moving when throwing
        if (Input.GetButtonDown("Fire1") && grounded)
        {
            throwing = true;
            SpawnPokeballToBone();
            playerAnimator.SetBool("IsThrowing", true);
        }

        //Not throwing
        if (!throwing)
        {
            //Regular walking & jumping
            if (Input.GetKey(KeyCode.LeftShift) && playerAnimator.GetBool("IsWalking"))
            {
                controller.Move(movement * runSpeed * Time.deltaTime);
                playerAnimator.SetBool("IsRunning", true);
            }
            else
            {
                controller.Move(movement * speed * Time.deltaTime);
                playerAnimator.SetBool("IsRunning", false);
            }

            //gravity and jumping
            velocity.y += gravity * Time.deltaTime;
            if (Input.GetButtonDown("Jump") && grounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight);
            }

            controller.Move(velocity * Time.deltaTime);

            playerAnimator.SetBool("IsJumping", !grounded);

            
        }
        //rotate alongside the Cam
        playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);


    }

    public void ThrowEnded()
    {
        throwing = false;
        playerAnimator.SetBool("IsThrowing", false);
    }

    private void SpawnPokeballToBone()
    {
        if (instantiatedPokeball == null)
        {
            instantiatedPokeball = Instantiate(pokeballPF, pokeBallBone, false);
        }
        
    }

    public void ReleasePokeball()
    {
        if (instantiatedPokeball != null)
        {
            instantiatedPokeball.transform.parent = null;
            instantiatedPokeball.GetComponent<SphereCollider>().enabled = true;
            instantiatedPokeball.GetComponent<Rigidbody>().useGravity = true;
            Transform cameraTransform = camera.transform;
            Vector3 throwAdjustment = new Vector3(0f, 0.5f, 0f);
            Vector3 throwVector = (cameraTransform.forward + throwAdjustment) * throwStrength;
            instantiatedPokeball.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.Impulse);
            instantiatedPokeball = null;
        }
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
}
