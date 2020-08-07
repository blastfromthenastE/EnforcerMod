﻿using RoR2;
using System;
using System.Collections.Specialized;
using UnityEngine;

public class ShieldComponent : MonoBehaviour
{
    static float maxSpeed = 0.1f;
    static float coef = 1; // affects how quickly it reaches max speed

    public bool isShielding = false;
    public Ray aimRay;
    public Vector3 shieldDirection = new Vector3(1,0,0);
    float initialTime = 0;

    private GameObject energyShield;
    private EnergyShieldControler energyShieldControler;

    private Transform _shieldPreview;
    private Transform _shieldParent;
    private float _shieldSize;
    private float _shieldSizeMultiplier = 1.2f;

    GameObject dummy;
    GameObject boyPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/LemurianBody");

   public float shieldHealth {
        get => energyShieldControler.healthComponent.health;
    }

    void Start()
    {
        var childLocator = GetComponentInChildren<ChildLocator>();
        childLocator.FindChild("EnergyShield").gameObject.SetActive(true);// i don't know if the object has to be active to get the component but i'm playing it safe
        energyShieldControler = childLocator.FindChild("EnergyShield").GetComponentInChildren<EnergyShieldControler>();
        energyShield = childLocator.FindChild("EnergyShield").gameObject;
        childLocator.FindChild("EnergyShield").gameObject.SetActive(false);
    }

    void Update() {

        aimShield();

        if (energyShieldControler) energyShieldControler.shieldAimRayDirection = aimRay.direction;

    }

    private void aimShield() {

        float time = Time.fixedTime - initialTime;

        Vector3 cross = Vector3.Cross(aimRay.direction, shieldDirection);
        Vector3 turnDirection = Vector3.Cross(shieldDirection, cross);

        float turnSpeed = maxSpeed * (1 - Mathf.Exp(-1 * coef * time));

        shieldDirection += turnSpeed * turnDirection.normalized;
        shieldDirection = shieldDirection.normalized;

        Vector3 difference = aimRay.direction - shieldDirection;
        if (difference.magnitude < 0.05) {
            initialTime = Time.fixedTime;
        }

        //displayShieldPreviewCube();
    }

    public void ToggleEnergyShield(bool shieldToggle)
    {
        if (energyShield) energyShield.SetActive(shieldToggle);
    }
}