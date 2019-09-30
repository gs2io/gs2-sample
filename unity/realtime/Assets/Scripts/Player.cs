using System;
using System.Collections;
using Google.Protobuf;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Realtime
{
    public class Player : MonoBehaviour
    {
        public float moveSpeed;
        private Vector2 _vector;
        private Text _textMesh;

        public RelayRealtimeSession Session;
        public Messenger Messenger;

        private void Start()
        {
            _textMesh = GetComponent<Text>();
            
            _vector.Set(
                transform.position.x,
                transform.position.y
            );
        }

        void Update () {
            if(Input.GetMouseButtonDown(0)){
                _vector = Input.mousePosition; 
            }

            var step = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(
                transform.position,
                new Vector2(_vector.x, _vector.y), 
                step
            );
        }

        public IEnumerator SendPosition()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.3f);
                if(_textMesh == null) continue;
                
                ByteString binary = null;
                try
                {
                    binary = ByteString.CopyFrom(Serialize());
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    continue;
                }

                if (Session != null)
                {
                    bool lockWasTaken = false;
                    try
                    {
                        System.Threading.Monitor.TryEnter(this, ref lockWasTaken);

                        if (lockWasTaken)
                        {
                            yield return Session.UpdateProfile(
                                r => { },
                                binary
                            );
                        }
                    }
                    finally
                    {
                        if (lockWasTaken) System.Threading.Monitor.Exit(this);
                    }
                }
            }
        }

        public byte[] Serialize()
        {
            var x = BitConverter.GetBytes(transform.position.x);
            var y = BitConverter.GetBytes(transform.position.y);
            var vectorX = BitConverter.GetBytes(_vector.x);
            var vectorY = BitConverter.GetBytes(_vector.y);
            var name = System.Text.Encoding.UTF8.GetBytes(_textMesh.text);

            int pos = 0;
            byte[] buffer = new byte[x.Length + y.Length + vectorX.Length + vectorY.Length + name.Length];
            Buffer.BlockCopy(x, 0, buffer, pos, x.Length);
            pos += x.Length;
            Buffer.BlockCopy(y, 0, buffer, pos, y.Length);
            pos += y.Length;
            Buffer.BlockCopy(vectorX, 0, buffer, pos, vectorX.Length);
            pos += vectorX.Length;
            Buffer.BlockCopy(vectorY, 0, buffer, pos, vectorY.Length);
            pos += vectorY.Length;
            Buffer.BlockCopy(name, 0, buffer, pos, name.Length);
            pos += name.Length;

            return buffer;
        }
    }
}