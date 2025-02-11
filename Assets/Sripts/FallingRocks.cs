﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRocks : MonoBehaviour {
    private GameObject[] rocks;
    private List<Rigidbody> rockBodies = new List<Rigidbody>();
    private GameObject moveTarget;
    private GameObject moveTarget2;
    private BoxCollider wall;
    private List<BoxCollider> rockPotsCols;

    private GameObject player;

    private CharacterController playerController;
    private Transform playerTransform;
    private PlayerMovement playerMovement;

    private Vector3 direction;

    private short step;
    private Transform moveTargetTransform;
    private Transform moveTarget2Transform;

    public float journeyTime = 1.0f;
    private float startTime;
    
    public bool started;
    public bool arrived;
    public bool done;

    public float timer = 3f;

    private AudioSource preSound;
    private AudioSource caveIn;

    private Game game;
    // Start is called before the first frame update
    void Start() {
        game = Game.SharedInstance;
        rocks = GameObject.FindGameObjectsWithTag("fallingRock");
        foreach (var rock in rocks) {
            rockBodies.Add(rock.GetComponent<Rigidbody>());
        }
        moveTarget = GameObject.Find("MoveTarget");
        moveTargetTransform = moveTarget.transform;
        moveTarget2 = GameObject.Find("MoveTarget2");
        moveTarget2Transform = moveTarget2.transform;
        wall = GameObject.Find("Wall").GetComponent<BoxCollider>();
        wall.enabled = false;
        GameObject[] rockPots = GameObject.FindGameObjectsWithTag("rockPot");
        rockPotsCols = new List<BoxCollider>();
        foreach (var rockPot in rockPots) {
            BoxCollider col = rockPot.GetComponent<BoxCollider>();
            col.enabled = false;
            rockPotsCols.Add(col);
        }
        preSound = GetComponents<AudioSource>()[0];
        caveIn = GetComponents<AudioSource>()[1];
    }
    
    // Update is called once per frame
    void FixedUpdate() {
        if (done)
            return;
        if (step == 1) {
            if (game.rotateTowardTarget(moveTargetTransform, playerTransform, startTime, journeyTime)) {
                moveTowardTarget();
            }
        }
        else if (step == 2) {
            if (arrived) {
                startRotation2();
                return;
            }
            playerController.Move(direction * (playerMovement.maxSpeedSprint * Time.deltaTime));
        } else if (step == 3) {
            if (game.rotateTowardTarget(moveTarget2Transform, playerTransform, startTime, journeyTime)) {
                RocksFall();
            }
        } else if (step == 4) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                finishIt();
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (started)
            return;
        if (other.gameObject.CompareTag("player")) {
            started = true;
            game.disableControlls = true;
            player = other.gameObject;
            playerController = player.GetComponent<CharacterController>();
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
            preSound.Play();
            rockCollapseEvent();
        }
    }

    private void rockCollapseEvent() {
        startRotation1();
        // Vector3 smer = moveTarget.transform.position = player.transform.position;
    }

    private void startRotation1() {
        startTime = Time.time;
        playerController.enabled = false;
        step = 1;
    }
    
    private void startRotation2() {
        startTime = Time.time;
        playerController.enabled = false;
        journeyTime = 2f;
        step = 3;
    }
    
    // hvala forumom na https://forum.unity.com/threads/slowly-turning-towards-target.49919/
    // in pa https://docs.unity3d.com/ScriptReference/Vector3.Slerp.html
    // private void rotateTowardTarget(Transform targetTransform) {
    //     Vector3 targetLookAtPoint = targetTransform.position - playerTransform.position;
    //     targetLookAtPoint = new Vector3(targetLookAtPoint.x, playerTransform.position.y, targetLookAtPoint.z);
    //     targetLookAtPoint.Normalize();
    //     float fracComplete = (Time.time - startTime) / journeyTime;
    //     targetLookAtPoint = Vector3.Slerp(playerTransform.forward, targetLookAtPoint,
    //         fracComplete);
    //     targetLookAtPoint += playerTransform.position;
    //     targetLookAtPoint.y = playerTransform.position.y;
    //     player.transform.LookAt(targetLookAtPoint);
    //     if (fracComplete >= 1) {
    //         if (step == 1) {
    //             moveTowardTarget();
    //         } else if (step == 3) {
    //             RocksFall();
    //         }
    //     }
    // }
    
    private void moveTowardTarget() {
        step = 2;
        playerController.enabled = true;
        playerMovement = player.GetComponent<PlayerMovement>();
        direction = (moveTargetTransform.position - playerTransform.position);
        direction.Normalize();
    }
    
    private void RocksFall() {
        step = 4;
        foreach (var rockPotCol in rockPotsCols) {
            rockPotCol.enabled = true;
        }
        caveIn.Play();
        foreach (var rockBody in rockBodies) {
            rockBody.constraints = RigidbodyConstraints.None;
        }
    }

    private void finishIt() {
        step = 0;
        done = true;
        foreach (var rockBody in rockBodies) {
            rockBody.constraints = RigidbodyConstraints.FreezeAll;
        }
        // foreach (var rockPotCol in rockPotsCols) {
        //     rockPotCol.enabled = false;
        // }
        wall.enabled = true;
        playerController.enabled = true;
        game.disableControlls = false;
        game.startStopwatch();
    }
}