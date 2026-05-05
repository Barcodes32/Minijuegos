Shader "Custom/ScratchCard"
{
    /*
    ════════════════════════════════════════════════════
    ScratchCard.shader
    
    Shader para el efecto de rascado.
    Permite borrar (hacer transparente) píxeles de la capa plateada.
    
    INSTALACIÓN:
    1. Crear carpeta Assets/Shaders/
    2. Guardar este archivo como ScratchCard.shader
    3. El ScratchController lo referencia automáticamente
    ════════════════════════════════════════════════════
    */
    
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Scratch Color", Color) = (0.75, 0.75, 0.75, 1) // Plateado por defecto
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        // Sin culling, sin escritura en depth buffer
        Cull Off
        ZWrite Off
        
        // Blend: el alpha 0 = transparente (borrado), alpha 1 = opaco (sin raspar)
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };
            
            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };
            
            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Color;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Multiplicar color del vertex (usado por GL.Color para hacer transparente)
                fixed4 finalColor = texColor * _Color;
                finalColor.a *= i.color.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    // Fallback si el shader no es soportado
    Fallback "Sprites/Default"
}
