using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sizing:MonoBehaviour  {


    private static float ratio = 0.10f;

    //small model base measurements 
    private static float s_height = 64f;
    private static float s_waist = 25f;
    private static float s_hip = 35f;
    private static float s_bust = 33f;
    //large model base measurements
    private static float l_height = 70f;
    private static float l_waist = 25f;
    private static float l_hip = 35f;
    private static float l_bust = 33f;

    //Canadian/US womens sizing ref - http://www.sizeguide.net
    //2-4
    private static float _small_waist = 25f;
    private static float _small_hip   = 34.5f;
    private static float _small_bust  = 32.5f;

    private static float small_waist = 26f;
    private static float small_hip = 35.5f;
    private static float small_bust = 33.5f;
    //6-8
    private static float _med_waist = 26f;
    private static float _med_hip   = 36.5f;
    private static float _med_bust  = 34.5f;

    private static float med_waist = 27f;
    private static float med_hip = 37.5f;
    private static float med_bust = 35.5f;
    //10-12
    private static float _large_waist = 28f;
    private static float _large_hip   = 38.5f;
    private static float _large_bust  = 36.5f;

    private static float large_waist = 29.5f;
    private static float large_hip = 39.5f;
    private static float large_bust = 38f;


    public float ConvertInput(int section, float size) {
        float value = 0f;

        switch (section) {
            case 1://height
                value = (s_height - size);
                break;
            case 2://bust
                value = (s_bust - size);
                break;
            case 3://waist
                value = (s_waist - size);
                break;
            case 4://hip
                value = (s_hip - size);
                break;
            default:
                break;
        }

        return value;
    }

    public string RecommendedFit(float[] measurements){
        string fit = "No size";

            if (measurements[0] >= _small_bust || measurements[0] <= small_bust && measurements[1] >= _small_hip || measurements[1] <= small_hip && measurements[2] >= _small_waist || measurements[2] <= small_waist)
            {//small
                //fit = "Recommended size small between size 2-4";
                fit = "s";
                return fit;
            }

            if (measurements[0] >= _med_bust || measurements[0] <= med_bust && measurements[1] >= _med_hip || measurements[1] <= med_hip && measurements[2] >= _med_waist || measurements[2] <= med_waist)
            {//medium
                //fit = "Recommended size small or medium between size 6-8";
                fit = "m";
                return fit;
            }
            if (measurements[0] >= _large_bust || measurements[0] <= large_bust && measurements[1] >= _large_hip || measurements[1] <= large_hip && measurements[2] >= _large_waist || measurements[2] <=large_waist)
             {//large
                //fit = "Recommended size large between size 10-12";
                fit = "l";
                return fit;
            }

        return fit;
    }

}
