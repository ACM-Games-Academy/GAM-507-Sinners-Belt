using UnityEngine;

public class CharacterSounds : MonoBehaviour
{

    public AK.Wwise.Event Play_Gun;
    public AK.Wwise.Event Play_Footstep;
    public AK.Wwise.Event Play_Reload;
    public AK.Wwise.Event Play_DoorOpen;
    public AK.Wwise.Event Play_DoorClose;



    public void PlayGunSound()
    {
        Play_Gun.Post(gameObject);
    }

    public void PlayFootstepSound()
    {
        Play_Footstep.Post(gameObject);
    }

    public void PlayReloadSound()
    {
        Play_Reload.Post(gameObject);
    }

    public void PlayOpenDoorSound()
    {
        Play_DoorOpen.Post(gameObject);
    }

    public void PlayCloseDoorSound()
    {
        Play_DoorClose.Post(gameObject);
    }
}
