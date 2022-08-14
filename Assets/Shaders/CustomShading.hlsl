#ifndef CUSTOM_SHADING
#define CUSTOM_SHADING

Varyings CustomLitPassVertex(Attributes input) {
	return LitPassVertex(input);
}

half4 CustomLitPassFragment(Varyings input) : SV_Target {
	return LitPassFragment(input);
}

#endif