Shader "Backface Rendered Bumped Diffuse" {
Properties {
_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {

// Set up alpha blending
Blend SrcAlpha OneMinusSrcAlpha

// Render the back facing parts of the object.
// If the object is convex, these will always be further away
// than the front-faces.
Pass {
Cull Front
SetTexture [_MainTex] {
Combine Primary * Texture
}
}
// Render the parts of the object facing us.
// If the object is convex, these will be closer than the
// back-faces.
Pass {
Cull Back
SetTexture [_MainTex] {
Combine Primary * Texture
}
}
}

FallBack "Unlit/Transparent"
} 