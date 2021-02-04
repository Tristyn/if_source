using PlayFab;
using System;
using System.Collections.Generic;

public sealed class Character
{
    public struct Save
    {
        public string characterId;
        public bool registered;
    }

    public struct Profile
    {
        public int nextCharacterId;
    }

    public Save save;
    public Profile profile;


    public void EnsureRegistered(Action result, Action error)
    {
        if (!save.registered)
        {
            
        }
    }

    int GetNextCharacterId()
    {
        return profile.nextCharacterId++;
    }

    void SetNextCharacterId()
    {

    }
}
