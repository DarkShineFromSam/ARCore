using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using Lean.Common;
using TMPro;


public class ObjectGenerator : MonoBehaviour
{

    public GameObject[] SpawnedObject;                                              //array of objects
    public GameObject SpawnMarkerPrefab;
    private ARRaycastManager aRRaycastManager;
    private List<ARRaycastHit> hits;
    private GameObject SpawnMarker;
    [SerializeField] private GameObject SpawnObjectScroll;
    [SerializeField] private Camera ARCamera;
    [SerializeField] private GameObject SelectionPanel;
    [SerializeField] private TMP_Text SelectedTitle;
    [SerializeField] private TMP_Text SelectedDescription;
    private int SpawnObjectIndex = -1;
    private int[] SpawnObjectCount;
    private string[] SpawnObjectName;

    private List<Vector2> PointsStart;
    private List<Vector2> PointsEnd;

    bool select;
    private enum ObjectGeneratorState { Inactive, ActiveSpawn, SpawnReady, Selection }
    private ObjectGeneratorState currentState;
    private GameObject selectedObject;

    private List<GameObject> Objects;                                                // Создание массива откуда возьмем координаты созданных объектов

    private int switchCount = 1;


    Vector2 point1, point2, point3, point4, result;
    public bool works;
    SpawnedObject stats;

    void Start()
    {
        works = false;

        select = false;
        //variable setup
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();
        hits = new List<ARRaycastHit>();
        currentState = ObjectGeneratorState.Inactive;
        SpawnObjectCount = new int[SpawnedObject.Length];
        SpawnObjectName = new string[SpawnedObject.Length];
        for (int i = 0; i < SpawnedObject.Length; i++)
        {
            SpawnObjectCount[i] = 0;
            SpawnObjectName[i] = SpawnedObject[i].name;
        }

        //instantiate new spawn marker
        SpawnMarker = Instantiate(SpawnMarkerPrefab, new Vector3(0.0f, 0.0f, 0.0f), SpawnMarkerPrefab.transform.rotation);
        SpawnMarker.SetActive(false);

        //hide UI elements
        SpawnObjectScroll.SetActive(false);
        SelectionPanel.SetActive(false);

        Objects = new List<GameObject>();
        PointsStart = new List<Vector2>();
        PointsEnd = new List<Vector2>();
        
    }

    void Update()
    {
        var fingers = Lean.Touch.LeanTouch.Fingers;
        int touchCount = Input.touchCount;


        if (touchCount <= 0)
            return;

        // process touch

        Touch touch = Input.GetTouch(0);
        Vector2 touchPosition = touch.position;
        // process object movement and rotation
        if (currentState == ObjectGeneratorState.Selection)
        {    
            if (touchCount == 1)                                            // перемещение  одним  пальцем
            {
               
                //MoveSelectedObject(touch);                  
            }

            else if (touchCount == 2)                                       // поворот 2 пальцами
            {
                RotateSelectedObject(touch, Input.GetTouch(1));                
            }

        }

        // process object selection
        else if (currentState == ObjectGeneratorState.Inactive)
        {
            TrySelectObject(touchPosition);
        }

        // process object creation

        if (currentState == ObjectGeneratorState.ActiveSpawn || currentState == ObjectGeneratorState.SpawnReady)
        {
            if (touch.phase == TouchPhase.Began)
            {
                ShowMarker(true);
                MoveMarker(touch.position);

            }
            else if (touch.phase == TouchPhase.Moved)
            {
                MoveMarker(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (currentState == ObjectGeneratorState.SpawnReady)
                {
                    SpawnObject();

                }
                else if (currentState == ObjectGeneratorState.ActiveSpawn)
                {
                    currentState = ObjectGeneratorState.SpawnReady;
                }
                ShowMarker(false);
            }
        }

    }

    public void TrySelectObject(Vector2 pos)                                        // Выбор объекта на сцене
    {
        Ray ray = ARCamera.ScreenPointToRay(pos);                   // стреляем лучем
        RaycastHit hitObject;

        if (Physics.Raycast(ray, out hitObject))
        {
            if (hitObject.collider.CompareTag("SpawnedObject"))
            {
                select = true;
                selectedObject = hitObject.collider.gameObject;
                stats = selectedObject.GetComponent<SpawnedObject>();

                if (stats != null)
                {
                    currentState = ObjectGeneratorState.Selection;
                    SelectionPanel.SetActive(true);
                    SelectedTitle.text = stats.Name;
                    RefreshDesc();
                }
                else
                {
                    Debug.Log("it broke :(");
                }
            }
        }
    }

    private void MoveSelectedObject(Touch touch)                                    // перемещение объекта по тачу
    {
        if (touch.phase == TouchPhase.Moved)
        {
            aRRaycastManager.Raycast(touch.position, hits, TrackableType.Planes);
            selectedObject.transform.position = hits[0].pose.position;
            switchCount = 1;
            DescrintionBut();
        }
    }

    private void RotateSelectedObject(Touch touch1, Touch touch2)                   // поворот объекта по 2 тачам
    {
        if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float distance = Vector2.Distance(touch1.position, touch2.position);
            float distancePrev = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);  // предыдущие тачи
            float delta = distance - distancePrev;
            switchCount = 3;
            if (delta > 0.0f)
            {
                delta *= 0.1f;              //affects the rotation speed
                selectedObject.transform.rotation *= Quaternion.Euler(0.0f, -touch1.deltaPosition.x * delta, 0.0f);
                DescrintionBut();
            }
            else if (delta < 0.0f)
            {
                delta *= 0.1f;              //affects the rotation speed
                selectedObject.transform.rotation *= Quaternion.Euler(0.0f, touch1.deltaPosition.x * delta, 0.0f);
                DescrintionBut();
            }
            else
            {
                return;
            }
        }
    }

    public void AutoRotate(float distance)                                          // вращение по свайпу (Swipe on distance)
    {
        if (select)
        {
            selectedObject.GetComponent<SpawnedObject>().speed += 30f;
        }
        
    }       

    public void DeleteObject()                                                      // повесили на кнопку Del
    {
        Objects.Remove(selectedObject);
        Destroy(selectedObject);
        CloseSelection();
    }

    public void ScalePlus()                                                         // Увеличиваем масштаб объекта
    {
        selectedObject.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
        switchCount = 2;
        DescrintionBut();
    }

    public void ScaleMinus()                                                        // Уменьшаем масштаб объекта
    {
        selectedObject.transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);
        switchCount = 2;
        DescrintionBut();
    }

    public void TurnRight()                                                         // Поворачиваем объект по часовой стрелке
    {
        selectedObject.transform.Rotate(0f, 0f, -15f);
        switchCount = 3;
        DescrintionBut();
    }

    public void TurnLeft()                                                          // поворачиваем объект против часовой стрелки
    {
        selectedObject.transform.Rotate(0f, 0f, 15f);
        switchCount = 3;
        DescrintionBut();
    }

    public void DescrintionBut()                                                    // Описание объектов
    {
        switch (switchCount)
        {
            case 1:
                SelectedDescription.text = "Position: \r\n" +
               "X: " + selectedObject.transform.position.x.ToString() + "\r\n" +
               "Y: " + selectedObject.transform.position.y.ToString() + "\r\n" +
               "Z: " + selectedObject.transform.position.z.ToString();
                break;

            case 2:
                SelectedDescription.text = "Scale: \r\n" +
                "X: " + selectedObject.transform.lossyScale.x.ToString() + "\r\n" +
                "Y: " + selectedObject.transform.lossyScale.y.ToString() + "\r\n" +
                "Z: " + selectedObject.transform.lossyScale.z.ToString();
                break;

            case 3:
                SelectedDescription.text = "Rotation: \r\n" +
                "X: " + selectedObject.transform.rotation.eulerAngles.x.ToString() + "\r\n" +
                "Y: " + selectedObject.transform.rotation.eulerAngles.y.ToString() + "\r\n" +
                "Z: " + selectedObject.transform.rotation.eulerAngles.z.ToString();

                break;

            default:
                SelectedDescription.text = $"Error!";
                break;
        }
    }
    public void RefreshDesc()
    {
        DescrintionBut();
        if (switchCount < 3)
            switchCount++;
        else
            switchCount = 1;

    }

    void ShowMarker(bool value)                                                     // Маркер
    {
        SpawnMarker.SetActive(value);
    }

    void MoveMarker(Vector2 touchPosition)
    {
        aRRaycastManager.Raycast(touchPosition, hits, TrackableType.Planes);
        SpawnMarker.transform.position = hits[0].pose.position;
    }

    private void SpawnObject()
    {
        bool canAdd = true;

        foreach (GameObject obj in Objects)
        {
            if (Vector3.Distance(SpawnMarker.transform.position, obj.transform.position) < 0.2)
            {
                canAdd = false;
            }
        }

        if (canAdd)
        {
            GameObject spawn = Instantiate(SpawnedObject[SpawnObjectIndex], SpawnMarker.transform.position, SpawnedObject[SpawnObjectIndex].transform.rotation);
            Objects.Add(spawn);
            currentState = ObjectGeneratorState.Inactive;
            SpawnObjectScroll.SetActive(false);

            SpawnObjectCount[SpawnObjectIndex]++;
            SpawnedObject stats = spawn.GetComponent<SpawnedObject>();
            stats.Name = SpawnObjectName[SpawnObjectIndex] + " " + SpawnObjectCount[SpawnObjectIndex].ToString();       // Название объекта
            stats.Discription = "Описание объекта " + stats.Name;       // Описание объекта
            stats.Spawn = spawn;
        }
    }

    public void ShowSelectionScroll()                                               // Вывод списка объектов
    {
            SpawnObjectScroll.SetActive(true);
            CloseSelection();
    }
    public void ChooseTypeCube()                                                    // Куб
    {
        SpawnObjectIndex = 0;
        currentState = ObjectGeneratorState.ActiveSpawn;
        SelectionPanel.SetActive(false);
    }
    public void ChooseTypeSphere()                                                  // Сфера
    {
        SpawnObjectIndex = 1;
        currentState = ObjectGeneratorState.ActiveSpawn;
        SelectionPanel.SetActive(false);
    }
    public void ChooseTypeCylinder()                                                // Цилинд
    {
        SpawnObjectIndex = 2;
        currentState = ObjectGeneratorState.ActiveSpawn;
        SelectionPanel.SetActive(false);
    }
    public void CloseSelection()                                                    // Закрывание описания
    {
        select = false;
        currentState = ObjectGeneratorState.Inactive;
        selectedObject = null;
        SelectionPanel.SetActive(false);
    }
    public void CloseSelectionOnDubleTap(int count)                                 // Закрывание описания (Tap on count)
    {
        if (currentState == ObjectGeneratorState.Selection)
        {
            if (count == 2)
            {
                CloseSelection();
                
            }
        }
    }                              

    public void PointDown(Vector2 OnScreen)
    {
        Debug.Log("OnScreen Down" + OnScreen);
        if (PointsStart.Count < 2)
        {
            PointsStart.Add(OnScreen);
        }
        else if (PointsStart.Count >= 2)
        {
            PointsStart[0] = PointsStart[1];
            PointsStart[1] = OnScreen;
        }
    }
    public void PointUp(Vector2 OnScreen)
    {
        Debug.Log("OnScreen up" + OnScreen);
        if (PointsEnd.Count < 2)
        {
            PointsEnd.Add(OnScreen);
        }
        else if (PointsEnd.Count >= 2)
        {
            PointsEnd[0] = PointsEnd[1];
            PointsEnd[1] = OnScreen;
        }
        if (PointsEnd.Count >1)
        {
            if (currentState == ObjectGeneratorState.Selection)
            {
                point1 = new Vector2(PointsStart[0].x, PointsStart[0].y);
                point2 = new Vector2(PointsEnd[0].x, PointsEnd[0].y);
                point3 = new Vector2(PointsStart[1].x, PointsStart[1].y);
                point4 = new Vector2(PointsEnd[1].x, PointsEnd[1].y);

                // try find intersection
                result = new Vector2(0.0f, 0.0f);
                works = LineIntersection(point1, point2, point3, point4, ref result);
                if (works)
                {
                    
                    Debug.Log("it works! intersection = " + result.ToString());
                    Objects.Remove(selectedObject);
                    
                    currentState = ObjectGeneratorState.Inactive;
                    selectedObject.GetComponent<SpawnedObject>().test = true;
                    stats = null;
                    CloseSelection();
                }
                else
                {
                    Debug.Log("there is no intersection!");
                    selectedObject.GetComponent<SpawnedObject>().test = false;
                }
            }
        }

       
    }

    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {
        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num, offset;
        float x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;

        // X bound box test/
        if (Ax < 0)
        {
            x1lo = p2.x; x1hi = p1.x;
        }
        else
        {
            x1hi = p2.x; x1lo = p1.x;
        }

        if (Bx > 0)
        {
            if (x1hi < p4.x || p3.x < x1lo) return false;
        }
        else
        {
            if (x1hi < p3.x || p4.x < x1lo) return false;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        // Y bound box test//
        if (Ay < 0)
        {
            y1lo = p2.y; y1hi = p1.y;
        }
        else
        {
            y1hi = p2.y; y1lo = p1.y;
        }

        if (By > 0)
        {
            if (y1hi < p4.y || p3.y < y1lo) return false;
        }
        else
        {
            if (y1hi < p3.y || p4.y < y1lo) return false;
        }

        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//

        // alpha tests//
        if (f > 0)
        {
            if (d < 0 || d > f) return false;
        }
        else
        {
            if (d > 0 || d < f) return false;
        }

        e = Ax * Cy - Ay * Cx;  // beta numerator//

        // beta tests //
        if (f > 0)
        {
            if (e < 0 || e > f) return false;
        }
        else
        {
            if (e > 0 || e < f) return false;
        }

        // check if they are parallel
        if (f == 0) return false;

        // compute intersection coordinates //
        num = d * Ax; // numerator //
        offset = same_sign(num, f) ? f * 0.5f : -f * 0.5f;   // round direction //
        intersection.x = p1.x + (num + offset) / f;

        num = d * Ay;
        offset = same_sign(num, f) ? f * 0.5f : -f * 0.5f;
        intersection.y = p1.y + (num + offset) / f;

        return true;
    }

    private static bool same_sign(float a, float b)
    {
        return ((a * b) >= 0f);
    }


}