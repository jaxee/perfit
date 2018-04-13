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
            case 1://height small
                value = (size - s_height);
                break;
            case 2://bust small
				value = (size - small_bust);
                break;
            case 3://waist small
				value = (size - small_waist);
                break;
            case 4://hip small
				value = (size - small_hip);
                break;

			case 5://height medium
				value = (size - s_height);
				break;
			case 6://bust medium
				value = (size - med_bust);
				break;
			case 7://waist medium
				value = (size - med_waist);
				break;
			case 8://hip medium
				value = (size - med_hip);
				break; 

			case 9://height large
				value = (size - s_height);
				break;
			case 10://bust large
				value = (size - large_bust);
				break;
			case 11://waist large
				value = (size - large_waist);
				break;
			case 12://hip large
				value = (size - large_hip);
				break;


            default:
                break;
        }

        return value;
    }

    public string RecommendedFit(float[] measurements){
        string fit = "No size";

            if (measurements[0] <= small_bust)
            {//small
				if (measurements [1] <= small_hip) {
					if (measurements [2] <= small_waist) {
						fit = "s";
						return fit;
					}
				}
            }

            if (measurements[0] <= med_bust)
            {//medium
				if(measurements[1] <= med_hip){
					if( measurements[2] <= _med_waist){
						fit = "m";
						return fit;
					}
				}
            }
            if (measurements[0] <= large_bust)
             {//large
				if(measurements[1] <= large_hip){
					if( measurements[2] >= _large_waist){
						fit = "l";
						return fit;
					}
				}
            }

        return fit;
    }

}
