// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using StarterAssets;
// using UnityEngine.UI;
// using TMPro;
// public class NPCSystem : MonoBehaviour
// {
//     bool is_player_nearby = false;
//     public GameObject d_template;
//     public GameObject canva;

//     private List<GameObject> currentDialogues = new List<GameObject>();
//     private int currentDialogueIndex = 0;

//     void Start()
//     {

//         if (canva != null)
//         {
//             canva.SetActive(false);
//         }
//     }
//     void NewDialogue(string text, bool showImmediately = false)
//     {

//         GameObject d_template_instance = Instantiate(d_template, canva.transform);
//         d_template_instance.SetActive(showImmediately);


//         TextMeshProUGUI textComponent = d_template_instance.GetComponentInChildren<TextMeshProUGUI>();
//         if (textComponent != null)
//         {
//             textComponent.text = text;
//         }
//         else
//         {
//             Debug.LogWarning("Không tìm thấy TextMeshProUGUI trong dialogue template!");
//         }


//         currentDialogues.Add(d_template_instance);
//     }

//     void ShowNextDialogue()
//     {
//         if (currentDialogueIndex < currentDialogues.Count)
//         {

//             if (currentDialogueIndex > 0)
//             {
//                 currentDialogues[currentDialogueIndex - 1].SetActive(false);
//             }


//             currentDialogues[currentDialogueIndex].SetActive(true);
//             currentDialogueIndex++;


//             if (currentDialogueIndex >= currentDialogues.Count)
//             {
//                 Debug.Log("All dialogues shown. Click again to close.");
//             }
//         }
//         else
//         {

//             EndDialogue();
//         }
//     }

//     void EndDialogue()
//     {

//         foreach (GameObject dialogue in currentDialogues)
//         {
//             if (dialogue != null)
//             {
//                 Destroy(dialogue);
//             }
//         }
//         currentDialogues.Clear();
//         currentDialogueIndex = 0;

//         StarterAssetsInputs.dialogue = false;
//         canva.SetActive(false);
//         Debug.Log("Dialogue ended - Player movement enabled");


//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;
//     }

//     void Update()
//     {
//         if (is_player_nearby && Input.GetKeyDown(KeyCode.E) && !StarterAssetsInputs.dialogue)
//         {

//             StarterAssetsInputs.dialogue = true;
//             canva.SetActive(true);
//             Debug.Log("Dialogue started - Player movement disabled");


//             Cursor.lockState = CursorLockMode.None;
//             Cursor.visible = true;


//             currentDialogueIndex = 0;


//             NewDialogue("Hello, this is a dialogue with the NPC.", true);
//             NewDialogue("This is another line of dialogue.", false);
//             NewDialogue("Click to see more...", false);
//             NewDialogue("This is the last dialogue.", false);


//             currentDialogueIndex = 1;
//         }


//         if (StarterAssetsInputs.dialogue && Input.GetMouseButtonDown(0))
//         {
//             ShowNextDialogue();
//         }
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Player") || other.name == "PlayerBody")
//         {
//             is_player_nearby = true;
//             Debug.Log("Player is nearby the NPC.");
//         }
//     }
//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Player") || other.name == "PlayerBody")
//         {
//             is_player_nearby = false;
//             Debug.Log("Player has left the NPC's vicinity.");
//         }
//     }
// }