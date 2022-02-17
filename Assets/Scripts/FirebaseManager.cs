using UnityEngine;
using UnityEditor;
using Proyecto26;
using System.Collections.Generic;
using UnityEngine.Networking;
using TMPro;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class FirebaseManager : MonoBehaviour
{

    public static FirebaseManager instance;
    public MenuManager menuManager;

    private string restLink = "https://ip-asg3-y2s2-default-rtdb.firebaseio.com";
    private string authKey = "AIzaSyB4iLqQhetQzvpCJIhczEZrRlF3daOsXJI"; //api key for firebase project 

    //account references
    public string idToken; //used for authenticating pushes to db
    public static string localId; //this is the unique account uid
    public string username;
    public static int userCompanyId = 0;
    public string companyName;

    //Image data & text reference
    public string imgData1 = "";
    public string imgData2 = "";
    public string imgData3 = "";

    public TextMeshPro imgText1;
    public TextMeshPro imgText2;
    public TextMeshPro imgText3;

    //Alert Reference
    public GameObject alertSaved;


    public TextMeshPro projectCount;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void GetProfile(string userId)
    {
        RestClient.Get(restLink + "/users/" + userId + ".json?auth=" + idToken).Then(response =>
        {
            User user = JsonUtility.FromJson<User>(response.Text);
            username = user.username;
            userCompanyId = user.companyId;
            GetCompanyName(userCompanyId);
        });
    }

    public void GetCompanyName(int companyId)
    {
        RestClient.Get(restLink + "/companys/" + companyId + ".json?auth=" + idToken).Then(response =>
        {
            Company company = JsonUtility.FromJson<Company>(response.Text);
            companyName = company.companyName;
        });
    }

    public void SavePhotoData(string imgData)
    {

        if (imgData1 == "")
        {
            imgData1 = imgData;
            imgText1.text = "Photo 1: Saved";

        } else if (imgData2 == "")
        {
            imgData2 = imgData;
            imgText2.text = "Photo 2: Saved";

        } else if (imgData3 == "")
        {
            imgData3 = imgData;
            imgText3.text = "Photo 3: Saved";
        } 
    }

    public void SaveProject(string projectName, string houseType, string roomNumber)
    {
        string creator = "Photo Take Test";
        string dateCreated = DateTime.Now.ToString("yyyy-MM-dd");
        int companyId = 0;
        string[] pictures = {imgData1, imgData2, imgData3}; //converts imagedata list into an array

        string[] furnitureUsed = { "Dyson Fan", "Chair" }; //converts list to array for uploading of data
        
        //creates json for upload
        Project newProject = new Project(companyId, creator, dateCreated, furnitureUsed, houseType, projectName, roomNumber, pictures);
        string link = restLink + "/projects/.json";
        RestClient.Post(link, newProject).Then(response => {
            Debug.Log(response.Text); //project id 
            alertSaved.SetActive(true);
            imgText1.text = "Photo 1: Empty";
            imgText2.text = "Photo 2: Empty";
            imgText3.text = "Photo 3: Empty";
            imgData1 = "";
            imgData2 = "";
            imgData3 = "";
        });
    }


    //function for creating account with firebase
    public void CreateAccount(string email, string username, string password)
    {

        string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
        RestClient.Post<SignResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=" + authKey, userData).Then( //then gets the response from Firebase on post entry
            response =>
            {
                idToken = response.idToken;
                localId = response.localId;

                //Create account in the RealtimeDb from inputs
                string link = restLink + "/users/" + localId + ".json?auth=" + idToken;
                User newUser = new User(email, username, localId);
                RestClient.Put(link, newUser);
                GetProfile(localId);
                menuManager.LoggedInAccount();
            }
       ).Catch(error =>
       {
           Debug.Log(error);
       });
    }

    //function for signing in user and getting their profile data
    public void SignInAccount(string email, string password)
    {
        Debug.Log(email + "/n" + password);
        string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
        Debug.Log(userData);
        RestClient.Post<SignResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=" + authKey, userData).Then(
            response =>
            {
                idToken = response.idToken;
                localId = response.localId;
                GetProfile(localId);

                menuManager.LoggedInAccount();

            }).Catch(error =>
            {
                Debug.Log(error);
            });
    }
}
