﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricCharacterRenderer : MonoBehaviour
{
    //public AudioClip soundToPlay;
    //public AudioSource audio;
    //public bool alreadyPlayed = false;

    public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };
    public static readonly string[] runDirections = { "Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };
    public static readonly string[] deadDirections = { "Dead N", "Dead NW", "Dead W", "Dead SW", "Dead S", "Dead SE", "Dead E", "Dead NE" };
    public static readonly string[] attackDirections = { "Attack N", "Attack NW", "Attack W", "Attack SW", "Attack S", "Attack SE", "Attack E", "Attack NE" };

    Animator animator;
    private int lastDirection;

    //Identification for separate attacks
    private short attackID;

    private void Awake()
    {
        //cache the animator component
        animator = GetComponent<Animator>();
    }

    public void SetDirection(Vector2 direction)
    {

        //use the Run states by default
        string[] directionArray = null;

        //measure the magnitude of the input.
        if (direction.magnitude < .01f)
        {
            //if we are basically standing still, we'll use the Static states
            //we won't be able to calculate a direction if the user isn't pressing one, anyway!
            directionArray = staticDirections;
            lastDirection = DirectionToIndex(direction, 8);
        }
        else
        {
            //we can calculate which direction we are going in
            //use DirectionToIndex to get the index of the slice from the direction vector
            //save the answer to lastDirection
            directionArray = runDirections;
            lastDirection = DirectionToIndex(direction, 8);
        }

        //tell the animator to play the requested state
        animator.Play(directionArray[lastDirection]);

    }

    public void SetStaticDirection(int ind)
    {
        // force to set a static direction

        lastDirection = ind;
        animator.Play(staticDirections[ind]);
    }

    //helper functions

    //this function converts a Vector2 direction to an index to a slice around a circle
    //this goes in a counter-clockwise direction.
    public static int DirectionToIndex(Vector2 dir, int sliceCount)
    {
        //get the normalized direction
        Vector2 normDir = dir.normalized;
        //calculate how many degrees one slice is
        float step = 360f / sliceCount;
        //calculate how many degress half a slice is.
        //we need this to offset the pie, so that the North (UP) slice is aligned in the center
        float halfstep = step / 2;
        //get the angle from -180 to 180 of the direction vector relative to the Up vector.
        //this will return the angle between dir and North.
        float angle = Vector2.SignedAngle(Vector2.up, normDir);
        //add the halfslice offset
        angle += halfstep;
        //if angle is negative, then let's make it positive by adding 360 to wrap it around.
        if (angle < 0)
        {
            angle += 360;
        }
        //calculate the amount of steps required to reach this angle
        float stepCount = angle / step;
        //round it, and we have the answer!
        return Mathf.FloorToInt(stepCount);
    }

    //this function converts a string array to a int (animator hash) array.
    public static int[] AnimatorStringArrayToHashArray(string[] animationArray)
    {
        //allocate the same array length for our hash array
        int[] hashArray = new int[animationArray.Length];
        //loop through the string array
        for (int i = 0; i < animationArray.Length; i++)
        {
            //do the hash and save it to our hash array
            hashArray[i] = Animator.StringToHash(animationArray[i]);
        }
        //we're done!
        return hashArray;
    }

    public void Attack()
    {
        animator.Play(attackDirections[lastDirection]);

        // attack audio
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        if(audioSource != null) AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);

        attackID++;
        attackID %= 100;
    }

    //Used By Controller to pause other animation untill Attack is finished playing
    public bool isPlayingAttack()
    {
        int animLayer = 0;
        return (animator.GetCurrentAnimatorStateInfo(animLayer).IsName(attackDirections[lastDirection]) &&
        animator.GetCurrentAnimatorStateInfo(animLayer).normalizedTime < 1.0f);
    }

    public short getAttackID()
    {
        return attackID;
    }

    public void Dead()
    {
        animator.Play(deadDirections[lastDirection]);

        // death audio
        //audio = gameObject.GetComponent<AudioSource>();

        //if (!alreadyPlayed) {
        //    audio.PlayOneShot(soundToPlay);
        //    alreadyPlayed = true;
        //}

        //AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
    }
}
