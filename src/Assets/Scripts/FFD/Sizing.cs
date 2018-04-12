using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sizing : MonoBehaviour {
    private static float ratio = 0.10f;

    //small model base measurements 
    private static float s_height = 160f;
    private static float s_waist = 25f;
    private static float s_hip = 35f;
    private static float s_bust = 33f;
    //large model base measurements
    private static float l_height = 160f;
    private static float l_waist = 25f;
    private static float l_hip = 35f;
    private static float l_bust = 33f;

    //Canadian/US womens sizing ref - http://www.sizeguide.net
    //2-4
    private static float small_waist = 25f;
    private static float small_hip   = 34.5f;
    private static float small_bust  = 32.5f;
    //6-8
    private static float med_waist = 26f;
    private static float med_hip   = 36.5f;
    private static float med_bust  = 34.5f;
    //10-12
    private static float large_waist = 28f;
    private static float large_hip   = 38.5f;
    private static float large_bust  = 36.5f;


    public float ConvertInput(int section, float size) {
        float value = 0f;

        switch (section) {
            case 1://heigt
                value = (s_height - size) * ratio;
                break;
            case 2://bust
                value = (s_bust - size) * ratio;
                break;
            case 3://waist
                value = (s_waist - size) * ratio;
                break;
            case 4://hip
                value = (s_hip - size) * ratio;
                break;
            default:
                break;
        }

        return value;
    }

    public string RecommendedFit(float[] measurements){
        string fit = "No size";

            if (measurements[0] <= small_bust && measurements[1] <= small_hip && measurements[2] <= small_waist)
            {//small
                fit = "Recommended size small between size 2-4";
                return fit;
            }

            if (measurements[0] <= med_bust && measurements[1] <= med_hip && measurements[2] <= med_waist)
            {//small - medium
                fit = "Recommended size small or medium between size 4-8";
                return fit;
            }
            if (measurements[0] <= large_bust && measurements[1] <= large_hip && measurements[2] <= large_waist)
            {//medium - large
                fit = "Recommended size medium or large between size 8-10";
                return fit;
            }

            if (measurements[0] >= large_bust && measurements[1] >= large_hip && measurements[2] >= large_waist)
            {//medium - large
                fit = "Recommended size large between size 10-12";
                return fit;
            }

        return fit;
    }

}
