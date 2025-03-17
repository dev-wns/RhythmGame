using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static PacketType;
public class LoginSystem : MonoBehaviour
{
    [Header( "< Login >" )]
    public GameObject loginCanvas;
    public TMP_InputField email;
    public TMP_InputField password;

    public PlayerInfo playerInfo;
    public TextMeshProUGUI message;

    private bool isFocusEmail;
    public event Action OnLoginCompleted;

    private void Awake()
    {
        // Protocols
        ProtocolSystem.Inst.Regist( CONFIRM_LOGIN_ACK,   AckConfirmMatchData );
        ProtocolSystem.Inst.Regist( CONFIRM_ACCOUNT_ACK, AckCreateUserData );
    }

    private void Start()
    {
        email.ActivateInputField();
        password.DeactivateInputField();
    }

    private void OnEnable()
    {
        isFocusEmail = false;
        message.text = string.Empty;

        // 패스워드 타입 설정( 코드로만 수정가능한 듯 )
        password.contentType = TMP_InputField.ContentType.Password;
    }

    public void Update()
    {
        if ( loginCanvas.activeInHierarchy )
        {
            if ( Input.GetKeyDown( KeyCode.Tab ) )
            {
                if ( isFocusEmail )
                {
                    email.ActivateInputField();
                    password.DeactivateInputField();
                }
                else
                {
                    email.DeactivateInputField();
                    password.ActivateInputField();
                }
                isFocusEmail = !isFocusEmail;
            }

            if ( Input.GetKeyDown( KeyCode.Return ) )
                 ReqConfirmLoginInfo();
        }
    }


    #region Button Events
    public void ActiveSignUpPanel( bool _isActive )
    {
        //signUpPanel.SetActive( _isActive );
        AudioManager.Inst.Play( SFX.MenuClick );

        if ( _isActive )
            email.ActivateInputField();
    }

    #region Request Protocols
    public void ReqConfirmLoginInfo()
    {
        Network.Inst.Send( new Packet( CONFIRM_LOGIN_REQ, new USER_INFO( email.text, password.text ) ) );
        AudioManager.Inst.Play( SFX.MenuClick );
    }

    public void ReqConfirmAccountInfo()
    {
        if ( email.text == string.Empty )
            return;

        Network.Inst.Send( new Packet( CONFIRM_ACCOUNT_REQ, new USER_INFO( email.text, password.text ) ) );
    }
    #endregion
    #endregion

    #region Response Protocols
    public void AckCreateUserData( Packet _packet )
    {
        AudioManager.Inst.Play( SFX.MenuClick );
        switch ( _packet.error )
        {
            case Error.DB_ERR_DISCONNECTED: break;
            case Error.OK:
            {
                message.text = "회원가입 완료";
            } break;

            case Error.DB_ERR_DUPLICATE_DATA:
            {
                message.text = "중복된 아이디 입니다.";
            } break;
            
            default:
            {
                Debug.LogWarning( _packet.error.ToString() );
            } break;
        }
    }

    private void AckConfirmMatchData( Packet _packet )
    {
        switch ( _packet.error )
        {
            case Error.DB_ERR_DISCONNECTED: break;
            case Error.OK:
            {
                var data = Packet.FromJson<USER_INFO>( _packet );
                GameManager.UserInfo = data;

                playerInfo.UpdateUserInfo( data );
                loginCanvas.SetActive( false );
                OnLoginCompleted?.Invoke();
            }
            break;

            default:
            {
              message.text = "아이디 또는 비밀번호가 일치하지 않습니다.";
            }
            break;
        }
    }
    #endregion
}