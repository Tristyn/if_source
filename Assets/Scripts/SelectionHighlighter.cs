using UnityEngine;

public class SelectionHighlighter : MonoBehaviour
{
    private void Awake()
    {
        Highlight(null);
    }

    public void Highlight(Conveyor selection)
    {
        if (selection)
        {
            transform.position = selection.transform.position;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
