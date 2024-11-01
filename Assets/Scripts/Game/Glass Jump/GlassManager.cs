using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class GlassManager : MonoBehaviourPunCallbacks
{
    public List<Glass> glasses;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetRandomBreakableStates();
            photonView.RPC("SyncGlassStates", RpcTarget.All, SerializeBreakableStates());
        }
    }

    private void SetRandomBreakableStates()
    {
        for (int i = 0; i < glasses.Count; i += 2)
        {
            glasses[i].breakable = Random.value < 0.5f;
            glasses[i + 1].breakable = !glasses[i].breakable;
        }
    }

    private string SerializeBreakableStates()
    {
        // Convert List<bool> to bool[] for serialization
        bool[] statesArray = glasses.ConvertAll(g => g.breakable).ToArray();
        return JsonUtility.ToJson(new BreakableStatesData { states = statesArray });
    }

    private void DeserializeBreakableStates(string json)
    {
        var data = JsonUtility.FromJson<BreakableStatesData>(json);
        for (int i = 0; i < glasses.Count; i++)
        {
            glasses[i].breakable = data.states[i];
        }
    }

    [PunRPC]
    private void SyncGlassStates(string json)
    {
        DeserializeBreakableStates(json);
    }

    [System.Serializable]
    private class BreakableStatesData
    {
        public bool[] states;
    }
}
