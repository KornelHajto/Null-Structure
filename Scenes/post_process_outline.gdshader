shader_type spatial;
render_mode unshaded, cull_disabled;

// Settings you can tweak in the Inspector
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D depth_texture : hint_depth_texture, filter_linear_mipmap;
uniform float outline_thickness : hint_range(0.0, 5.0) = 2.0;
uniform float sensitivity : hint_range(0.0, 100.0) = 50.0;
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void vertex() {
	// Cover the entire screen
	POSITION = vec4(VERTEX, 1.0);
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 pixel_size = 1.0 / VIEWPORT_SIZE;
	
	// Sample depth at the current pixel and its neighbors
	float d_center = texture(depth_texture, uv).r;
	float d_up     = texture(depth_texture, uv + vec2(0.0, pixel_size.y) * outline_thickness).r;
	float d_down   = texture(depth_texture, uv - vec2(0.0, pixel_size.y) * outline_thickness).r;
	float d_left   = texture(depth_texture, uv + vec2(pixel_size.x, 0.0) * outline_thickness).r;
	float d_right  = texture(depth_texture, uv - vec2(pixel_size.x, 0.0) * outline_thickness).r;
	
	// Calculate the difference in depth (Sobel-ish edge detection)
	float depth_diff = abs(d_center - d_up) + abs(d_center - d_down) + abs(d_center - d_left) + abs(d_center - d_right);
	
	// Linearize depth for better consistency across distances (Optional, but helps)
	// Note: In Godot 4, raw depth is non-linear. For a simple outline, raw depth usually works fine 
	// if you tune sensitivity.
	
	// Draw the game normally first
	vec3 original_color = texture(screen_texture, uv).rgb;
	
	// If edge detected, draw outline. Otherwise, draw original scene.
	// We multiply depth_diff to make it sensitive enough.
	if (depth_diff * 1000.0 > sensitivity) {
		ALBEDO = outline_color.rgb;
	} else {
		ALBEDO = original_color;
	}
}