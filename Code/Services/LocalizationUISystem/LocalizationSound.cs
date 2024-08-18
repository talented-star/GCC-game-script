//using PixelCrushers;
//using UnityEngine;
//using UnityEngine.UI;

//public class LocalizationSound : LocObject
//{
//    [SerializeField] private StringField t_SoundElement;

//    public StringField TSoundElement
//    {
//        get
//        {
//            return t_SoundElement;
//        }

//        set
//        {
//            t_SoundElement = value;
//        }
//    }

//    private void OnEnable()
//    {
//        //LanguageSettings.language += OnSetLanguage;
//        //OnSetLanguage();
//    }

//    private void OnDisable()
//    {
//        //LanguageSettings.language -= OnSetLanguage;
//    }

//    public AudioClip GetClip(string key)
//    {
//        return Resources.Load<AudioClip>(key);
//    }

//    public override void OnSetLanguage()
//    {
//        SetLanguage(this.GetComponent<Image>(), t_SoundElement.value);
//    }

//    public static Image SetLanguage(Image to, string key)
//    {
//        AudioClip sprite = Resources.Load<AudioClip>(key);
//        //to.sprite = sprite;
//        return to;
//    }
//}
