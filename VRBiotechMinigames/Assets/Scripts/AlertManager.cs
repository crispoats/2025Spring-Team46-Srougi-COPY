using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
//author: Mark Guo
//editor: Jason Nguyen
public class AlertManager : MonoBehaviour
{
    [SerializeField] public Notebook notebook;    // Reference to Notebook
    private Alert currentAlert;

    [SerializeField]
    private AudioSource alertSFX;

    private void Awake()
    {
        MistakeEvent.GetInstance().AddListener(DisplayAlert);

        alertSFX = gameObject.AddComponent<AudioSource>();
        AudioClip clip = Resources.Load<AudioClip>("Audio/AlertSoundSFX");
        if (clip != null)
        {
            alertSFX.clip = clip;
        }
    }

    /// Returns the currently active alert.
    public Alert GetCurrentAlert()
    {
        return currentAlert;
    }

    /// Displays an alert based on a given Mistake, creating an Alert internally.
    /// Then notifies the Notebook of the mistake.
    public void DisplayAlert(MistakeEventArgs mistake)
    {
        GameObject obj = Instantiate(Resources.Load("AlertCanvas", typeof(GameObject))) as GameObject;
        Alert alert = obj.GetComponent<Alert>();
        if (alert == null) return;
        alert.SetTransform(mistake.getTransform());
        alert.SetMessage(MistakeEvent.getMistakeDescription(mistake.getMessageKey()));
        alert.SetOffset(mistake.getOffset());
        
        if (currentAlert != null)
        {
            DismissAlert();
        }
        currentAlert = alert;
        alert.Display();
        NotifyNotebook(mistake);
    }

    /// Dismisses the current alert with a fade-out effect.
    public void DismissAlert()
    {
        if (currentAlert != null)
        {
            currentAlert.Dismiss();
            currentAlert = null;
        }
       
    }

    /// Notifies the Notebook to log the Mistake.
    private void NotifyNotebook(MistakeEventArgs mistakeArgs)
    {
        if (notebook != null)
        {
            notebook.AddMistake(mistakeArgs);
            alertSFX.Play();
        }
    }
}
