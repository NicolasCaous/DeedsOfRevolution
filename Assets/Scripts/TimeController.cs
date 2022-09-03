using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public Transform dayNightTransform;
    public Transform seasonsTransform;
    public string initialDateTime;
    public string summerSolsticeDateTime;
    public DateTime dateTime;

    public double[] speedTicksPerSecond = new double[] {
        1f, 3f, 6f, 12f, 24f, 48f
    };
    public int speed = 0;
    public bool pause = false;

    private DateTime initialDateTimeDT;
    private DateTime summerSolsticeDateTimeDT;

    private int tick = 0;
    private double lastTick = 0;

    void Start()
    {
        lastTick = Time.realtimeSinceStartup;

        CultureInfo culture = CultureInfo.CreateSpecificCulture("pt-BR");
        initialDateTimeDT = DateTime.Parse(initialDateTime, culture);
        summerSolsticeDateTimeDT = DateTime.Parse(summerSolsticeDateTime, culture);

        UpdateTickValues();
    }

    void Update()
    {
        if (pause)
        {
            lastTick += Time.deltaTime;
            return;
        }

        double now = Time.realtimeSinceStartup;
        double timeSinceLastTick = now - lastTick;
        double nextTickProgress = timeSinceLastTick / (1f / speedTicksPerSecond[speed]);

        double hourOfTheDay = (tick % 24) + nextTickProgress;
        float hourOfTheDayAngle = (float)(180f - hourOfTheDay * 15f);

        if (nextTickProgress >= 1f)
        {
            ++tick;
            lastTick = now;
            UpdateTickValues();
        }

        dayNightTransform.localRotation = Quaternion.Euler(
            hourOfTheDayAngle,
            0f,
            0f
        );
    }

    private void UpdateTickValues()
    {
        dateTime = initialDateTimeDT.AddHours(tick);

        if (dateTime.Year != summerSolsticeDateTimeDT.Year)
            summerSolsticeDateTimeDT.AddYears(1);

        int daysInYear = DateTime.IsLeapYear(dateTime.Year) ? 366 : 365;
        double yearProgress = (double) dateTime.DayOfYear / (double) daysInYear;
        double summerSolsticeOffset = (double) summerSolsticeDateTimeDT.DayOfYear / (double) daysInYear;

        double seasonProgress = yearProgress - summerSolsticeOffset;
        if (seasonProgress < 0f)
            seasonProgress += 1f;

        float seasonAngle = Mathf.Sin(Mathf.Lerp(-Mathf.PI / 2f, (3f * Mathf.PI) / 2f, (float)seasonProgress)) * 23.44f;
        seasonsTransform.localRotation = Quaternion.Euler(
            0f,
            0f,
            seasonAngle
        );
    }
}
