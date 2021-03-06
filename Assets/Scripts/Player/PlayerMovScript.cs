﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovScript : MonoBehaviour
{
    public delegate void Attack(ref float energy);

    public Attack playerAttack;
    public Animator anim;

    public string playerBeginning = "Joy";
    public int playerNumber = 1;

    public string[] buttonEndings = { "Start" };
    public string[] axisEndings = { "Horizontal", "Vertical", "Shoot", "Horizontal2", "Vertical2" };

    public float moveSpeed = 3;
    public float moveSpeedShooting = 2;
    public bool Shooting = false;

    public bool useController = true;

    public CharacterController controller;
    public Vector3 direction;
    public Vector3 lookDir;

    public Vector2 screenCenter;

    float playerHeight;
    public Vector3 localVelocity;

    public float maxEnergy;
    [HideInInspector] public float energy;
    public float EnergyPerTick;

    public GameObject[] Guns;

    // Use this for initialization
    void Start ()
    {
        playerBeginning = playerBeginning + playerNumber.ToString();

        // Getting player controller
        controller = GetComponent<CharacterController>();
        playerHeight = transform.position.y + controller.stepOffset;
        playerAttack = GetComponent<Rifle>().Attack;

        energy = maxEnergy;
    }
    void Awake()
    {
        playerAttack = null;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, playerHeight, transform.position.z);
        Shooting = false;

        for(int i = 0; i < Guns.Length; i++)
        {
            Guns[i].transform.rotation = transform.rotation;
            Guns[i].transform.Rotate(0, -90, 0);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        // transform.position = new Vector3(transform.position.x, playerHeight, transform.position.z);

        localVelocity = transform.InverseTransformDirection(GetComponent<CharacterController>().velocity);
        // Debug.Log(transform.InverseTransformDirection(GetComponent<CharacterController>().velocity));

        anim.SetFloat("Horizontal", localVelocity.x);
        anim.SetFloat("Vertical", localVelocity.z);

        bool horBigger = false;
        bool verBigger = false;

        if (Mathf.Abs(localVelocity.x) == 0 &&
            Mathf.Abs(localVelocity.y) == 0)
        {

        }
        else if (Mathf.Abs(localVelocity.x) > Mathf.Abs(localVelocity.z))
        {
            horBigger = true;
        }
        else
        {
            verBigger = true;
        }

        anim.SetBool("HorizontalBigger", horBigger);
        anim.SetBool("VerticalBigger", verBigger);

        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (!GetComponent<Health>().NeedRes)
        {

            if (useController)
            {
                CheckKeys();
            }
            else
            {
                // Mouse
                RaycastHit hit;

                int mask = 1 << LayerMask.NameToLayer("CursorRaycast");

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, mask))
                {
                    transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }

                if (Input.GetMouseButton(0) && playerAttack != null)
                    playerAttack(ref energy);


                // Keyboard
                direction = Vector3.zero;
                Vector3 forward = Vector3.Cross(Camera.main.transform.right, Vector3.up);

                if (Input.GetKey(KeyCode.W))
                    direction += forward * moveSpeed * Time.deltaTime;
                if (Input.GetKey(KeyCode.S))
                    direction -= forward * moveSpeed * Time.deltaTime;

                if (Input.GetKey(KeyCode.A))
                    direction -= Camera.main.transform.right * moveSpeed * Time.deltaTime;
                if (Input.GetKey(KeyCode.D))
                    direction += Camera.main.transform.right * moveSpeed * Time.deltaTime;

                controller.Move(direction);
            }
        }

        energy += EnergyPerTick;
        if (energy > maxEnergy)
            energy = maxEnergy;
    }

    void CheckKeys()
    {
        direction = Vector3.zero;
        lookDir = Vector3.zero;

        Vector2 lookScreenPos = screenCenter;
        Vector3 forward = Vector3.Cross(Camera.main.transform.right, Vector3.up);

        foreach (string axis in axisEndings)
        {
            string stringCombo = playerBeginning + axis;

            if (Input.GetAxis(stringCombo) >= 0.5)
            {
                if (axis == "Shoot")
                {
                    playerAttack(ref energy);
                    Shooting = true;
                }
            }
        }

        // Check all axis movements
        foreach (string axis in axisEndings)
        {
            string stringCombo = playerBeginning + axis;
            if (Input.GetAxis(stringCombo) > 0.2f ||
                Input.GetAxis(stringCombo) < -0.2f)
            {
                if (axis == "Horizontal")
                {
                    if (Shooting)
                        direction += (Camera.main.transform.right) * Input.GetAxis(stringCombo) * moveSpeedShooting * Time.deltaTime;
                    else
                        direction += (Camera.main.transform.right) * Input.GetAxis(stringCombo) * moveSpeed * Time.deltaTime;
                }
                else if (axis == "Vertical")
                {
                    if (Shooting)
                        direction -= forward * Input.GetAxis(stringCombo) * moveSpeedShooting * Time.deltaTime;
                    else
                        direction -= forward * Input.GetAxis(stringCombo) * moveSpeed * Time.deltaTime;
                }
            }

            if (Input.GetAxis(stringCombo) > 0.005 ||
                Input.GetAxis(stringCombo) < 0.005)
            {
                if (axis == "Horizontal2")
                {
                    lookScreenPos = new Vector2(Input.GetAxis(stringCombo), lookScreenPos.y);
                }
                else if (axis == "Vertical2")
                {
                    lookScreenPos = new Vector2(lookScreenPos.x, -Input.GetAxis(stringCombo));
                }
            }

            // lookScreenPos = GetComponent<CharacterController>().velocity;
        }

        // Check all button inputs
        foreach (string button in buttonEndings)
        {
            string stringCombo = playerBeginning + button;
            if (Input.GetButton(stringCombo))
            {

            }
        }

        // lookScreenPos = GetComponent<CharacterController>().velocity;
        
        // If there is input on the thumbsticks
        if (new Vector3(lookScreenPos.x, 0, lookScreenPos.y) != Vector3.zero)
        {
            lookDir = transform.forward = new Vector3(lookScreenPos.x, 0, lookScreenPos.y);
            transform.eulerAngles = transform.eulerAngles + new Vector3(0, 45, 0);
        }
        controller.Move(direction);
    }
}
