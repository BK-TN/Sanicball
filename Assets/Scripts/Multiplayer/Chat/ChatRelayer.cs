using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

public class ChatRelayer : NetworkBehaviour {

	const short CHAT_MSG = MsgType.Highest + 1; // Unique message ID
	public ChatBoxFunctions chat; // Separate, non-networked script handling the chat window/interface/GUI
	NetworkClient client;
	public InputField chatMessageField;
	int messageID ;
	public static ChatRelayer Instance;
	public Text consoleText;
	public void Awake(){
		if(Instance==null)
			Instance = this;
	}

	void Start() {

		NetworkManager netManager = GameObject.FindObjectOfType<NetworkManager>();
		client = netManager.client;
		if( client.isConnected )
			client.RegisterHandler( CHAT_MSG, ClientReceiveChatMessage );
		if( isServer )
			NetworkServer.RegisterHandler( CHAT_MSG, ServerReceiveChatMessage );

		chatMessageField.onEndEdit.AddListener(delegate{SetMessage();}) ;

		
	}

	public void SetMessage (){
		messageID = Random.Range(1,999999);
		var msg = new MyMessage();
		msg.stuff = "<color=yellow>[ " +Sanicball.Data.ActiveData.GameSettings.nickname+ " ]:</color> " + chatMessageField.text;
		msg.netId = messageID;
		SendChatMessage( msg);	
		chatMessageField.text="";

	}

	public void SetLogMessage (string log){

		var msg = new MyMessage();
		msg.stuff = "<color=lightblue>[ "+ log +" ]:</color>" ;
		msg.netId = 0;
		SendChatMessage( msg);	

	}

	public void SendChatMessage(MyMessage msg) {

		if( isServer ) {
			NetworkServer.SendToAll( CHAT_MSG, msg ); // Send to all clients
		} else if(client.isConnected ) {
			client.Send( CHAT_MSG, msg ); // Sending message from client to server
		}
	}

	public void ServerReceiveChatMessage( NetworkMessage netMsg ) {
//		string str = netMsg.ReadMessage<StringMessage>().value;
		MyMessage str= netMsg.ReadMessage<MyMessage>();
		if( isServer ) {
			SendChatMessage( str ); // Send the chat message to all clients
		}
	}

	public void ClientReceiveChatMessage( NetworkMessage netMsg ) {
		MyMessage message = netMsg.ReadMessage<MyMessage>();
		string pls=message.stuff;


		if( client.isConnected ) {
			if(message.netId==messageID) {
				pls= pls.Replace("<color=yellow>[ " ,"<color=cyan>[ " );
			}
			if(pls.Contains("De3bug_Consol_Lodg")){
				pls= pls.Replace("<color=lightblue>[ " ,"<color=white>[ " );
				pls = pls.Remove(13,20);
				pls = pls.Remove(pls.Length-10,2);

				if(pls.Contains("Spawning in..." )){
					NetworkManager.singleton.GetComponent<SanicNetworkManager>().isSpawning=true;
				}

				consoleText.text = pls;
					
			}else{// the message goes to the chat window
				chat.ShowMessage( pls ); // Add the message to the client's local chat window

			}
		}
	}

}

public class MyMessage : MessageBase
{
	public int netId;
	public string stuff;
	public bool MatchStart;
}