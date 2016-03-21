    Shader "Cull Off/Diffuse" {
        Properties {
            _Color ("Main Color", Color) = (1,1,1,0)
            _MainTex ("Base (RGB)", 2D) = "white" {}
        }
        SubShader {
            Pass {
                Cull Off
                Material {
                    Diffuse [_Color]
                    Ambient [_Color]
                }
                Lighting On
                SetTexture [_MainTex] {
                    Combine texture * primary DOUBLE, texture * primary
                }
            }
        }
    }