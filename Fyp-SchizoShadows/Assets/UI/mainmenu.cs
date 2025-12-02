using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public void start(){
        SceneManager.LoadSceneAsync(1);
    }
}

