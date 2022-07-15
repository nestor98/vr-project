// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/Atmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // Geometric parameters
        _PlanetRadius ("Planet Radius", Float) = 6371000.0
        _AtmosphereThickness ("Atmosphere Thickness", Float) = 100000.0

        // Performance parameters
        _PrimarySteps ("Primary Steps", int) = 12
        _LightSteps ("Light Steps", int) = 4

        // Atmosphere parameters
        _LightIntensity ("Light Intensity", Float) = 40.0
        _LightDir ("Light Direction", Vector) = (0, 1, 0, 0)
        _RayleighScattering ("Rayleigh Scattering Color", Color) = (0.245, 0.58, 1.0, 0)
        _RayleighScatteringScale ("Rayleigh Scattering Scale", Float) = 0.0000224
        _MieScatteringScale ("Mie Scattering Scale", Float) = 0.000021
        _AmbientScattering ("Ambient Scattering Color", Color) = (0, 0, 0, 0)
        _AmbientScatteringScale ("Ambient Scattering Scale", Float) = (0, 0, 0, 0)
        _OzoneAbsorption ("Rayleigh Absorption Color", Color) = (0.41, 1.0, 0.039, 0)
        _OzoneAbsorptionScale ("Ozone Absorption Scale", Float) = 0.0000497
        _RayleighScaleHeight ("Rayleigh Scale Height", Float) = 8000.0
        _MieScaleHeight ("Mie Scale Height", Float) = 1200.0
        _OzoneScaleHeight ("Ozone Scale Height", Float) = 30000.0
        _AbsorptionFalloff ("Absorption Falloff", Float) = 4000.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 view_ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                float3 view_ray = mul(unity_CameraInvProjection, float4(v.uv.xy * 2.0f - 1.0f, 0.0f, -1.0f));
				o.view_ray = normalize(mul(unity_CameraToWorld, float4(view_ray, 0.0f)));
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float _AtmosphereThickness;
            float _PlanetRadius;

            int _PrimarySteps;
            int _LightSteps;

            float _LightIntensity;
            float4 _LightDir;
            float4 _RayleighScattering;
            float _RayleighScatteringScale;
            float _MieScatteringScale;
            float4 _AmbientScattering;
            float4 _AmbientScatteringScale;
            float4 _OzoneAbsorption;
            float _OzoneAbsorptionScale;
            float _RayleighScaleHeight;
            float _MieScaleHeight;
            float _OzoneScaleHeight;
            float _AbsorptionFalloff;

            // https://www.shadertoy.com/view/wlBXWK
            // Original from scratchapixel
            float3 calculate_scattering(
                float3 start, 				// the start of the ray (the camera position)
                float3 dir, 			    // the direction of the ray (the camera vector)
                float max_dist, 			// the maximum distance the ray can travel (because something is in the way, like an object)
                float3 scene_color,			// the color of the scene
                float3 light_dir, 			// the direction of the light
                float3 light_intensity,		// how bright the light is, affects the brightness of the atmosphere
                float3 planet_position, 	// the position of the planet
                float planet_radius, 		// the radius of the planet
                float atmo_radius, 			// the radius of the atmosphere
                float3 beta_ray, 			// the amount rayleigh scattering scatters the colors (for earth: causes the blue atmosphere)
                float3 beta_mie, 			// the amount mie scattering scatters colors
                float3 beta_absorption,   	// how much air is absorbed
                float3 beta_ambient,		// the amount of scattering that always occurs, cna help make the back side of the atmosphere a bit brighter
                float g, 					// the direction mie scatters the light in (like a cone). closer to -1 means more towards a single direction
                float height_ray, 			// how high do you have to go before there is no rayleigh scattering?
                float height_mie, 			// the same, but for mie
                float height_absorption,	// the height at which the most absorption happens
                float absorption_falloff,	// how fast the absorption falls off from the absorption height
                int steps_i, 				// the amount of steps along the 'primary' ray, more looks better but slower
                int steps_l 				// the amount of steps along the light ray, more looks better but slower
                )
            {
                // add an offset to the camera position, so that the atmosphere is in the correct position
                start -= planet_position;
                // calculate the start and end position of the ray, as a distance along the ray
                // we do this with a ray sphere intersect
                float a = dot(dir, dir);
                float b = 2.0f * dot(dir, start);
                float c = dot(start, start) - (atmo_radius * atmo_radius);
                float d = (b * b) - 4.0f * a * c;
                
                // stop early if there is no intersect
                if (d < 0.0f) return scene_color;
                
                // calculate the ray length
                float2 ray_length = float2(
                    max((-b - sqrt(d)) / (2.0f * a), 0.0f),
                    min((-b + sqrt(d)) / (2.0f * a), max_dist)
                );
                
                // if the ray did not hit the atmosphere, return a black color
                if (ray_length.x > ray_length.y) return scene_color;
                // prevent the mie glow from appearing if there's an object in front of the camera
                bool allow_mie = max_dist > ray_length.y;
                // make sure the ray is no longer than allowed
                ray_length.y = min(ray_length.y, max_dist);
                ray_length.x = max(ray_length.x, 0.0f);
                // get the step size of the ray
                float step_size_i = (ray_length.y - ray_length.x) / float(steps_i);
                
                // next, set how far we are along the ray, so we can calculate the position of the sample
                // if the camera is outside the atmosphere, the ray should start at the edge of the atmosphere
                // if it's inside, it should start at the position of the camera
                // the min statement makes sure of that
                float ray_pos_i = ray_length.x + step_size_i * 0.5f;
                
                // these are the values we use to gather all the scattered light
                float3 total_ray = float3(0.0f, 0.0f, 0.0f); // for rayleigh
                float3 total_mie = float3(0.0f, 0.0f, 0.0f); // for mie
                
                // initialize the optical depth. This is used to calculate how much air was in the ray
                float3 opt_i = float3(0.0f, 0.0f, 0.0f);
                
                // also init the scale height, avoids some float2's later on
                float2 scale_height = float2(height_ray, height_mie);
                
                // Calculate the Rayleigh and Mie phases.
                // This is the color that will be scattered for this ray
                // mu, mumu and gg are used quite a lot in the calculation, so to speed it up, precalculate them
                float mu = dot(dir, light_dir);
                float mumu = mu * mu;
                float gg = g * g;
                float phase_ray = 3.0f / (50.2654824574f /* (16 * pi) */) * (1.0f + mumu);
                float phase_mie = allow_mie ? 3.0f / (25.1327412287f /* (8 * pi) */) * ((1.0f - gg) * (mumu + 1.0f)) / (pow(1.0f + gg - 2.0f * mu * g, 1.5f) * (2.0f + gg)) : 0.0f;
                
                // now we need to sample the 'primary' ray. this ray gathers the light that gets scattered onto it
                for (int i = 0; i < steps_i; ++i) {
                    
                    // calculate where we are along this ray
                    float3 pos_i = start + dir * ray_pos_i;
                    
                    // and how high we are above the surface
                    float height_i = length(pos_i) - planet_radius;
                    
                    // now calculate the density of the particles (both for rayleigh and mie)
                    float3 density = float3(exp(-height_i / scale_height), 0.0f);
                    
                    // and the absorption density. this is for ozone, which scales together with the rayleigh, 
                    // but absorbs the most at a specific height, so use the sech function for a nice curve falloff for this height
                    // clamp it to avoid it going out of bounds. This prevents weird black spheres on the night side
                    float denom = (height_absorption - height_i) / absorption_falloff;
                    density.z = (1.0f / (denom * denom + 1.0f)) * density.x;
                    
                    // multiply it by the step size here
                    // we are going to use the density later on as well
                    density *= step_size_i;
                    
                    // Add these densities to the optical depth, so that we know how many particles are on this ray.
                    opt_i += density;
                    
                    // Calculate the step size of the light ray.
                    // again with a ray sphere intersect
                    // a, b, c and d are already defined
                    a = dot(light_dir, light_dir);
                    b = 2.0f * dot(light_dir, pos_i);
                    c = dot(pos_i, pos_i) - (atmo_radius * atmo_radius);
                    d = (b * b) - 4.0f * a * c;

                    // no early stopping, this one should always be inside the atmosphere
                    // calculate the ray length
                    float step_size_l = (-b + sqrt(d)) / (2.0f * a * float(steps_l));

                    // and the position along this ray
                    // this time we are sure the ray is in the atmosphere, so set it to 0
                    float ray_pos_l = step_size_l * 0.5f;

                    // and the optical depth of this ray
                    float3 opt_l = float3(0.0f, 0.0f, 0.0f);
                        
                    // now sample the light ray
                    // this is similar to what we did before
                    for (int l = 0; l < steps_l; ++l) {

                        // calculate where we are along this ray
                        float3 pos_l = pos_i + light_dir * ray_pos_l;

                        // the heigth of the position
                        float height_l = length(pos_l) - planet_radius;

                        // calculate the particle density, and add it
                        // this is a bit verbose
                        // first, set the density for ray and mie
                        float3 density_l = float3(exp(-height_l / scale_height), 0.0f);
                        
                        // then, the absorption
                        float denom = (height_absorption - height_l) / absorption_falloff;
                        density_l.z = (1.0f / (denom * denom + 1.0f)) * density_l.x;
                        
                        // multiply the density by the step size
                        density_l *= step_size_l;
                        
                        // and add it to the total optical depth
                        opt_l += density_l;
                        
                        // and increment where we are along the light ray.
                        ray_pos_l += step_size_l;
                        
                    }
                    
                    // Now we need to calculate the attenuation
                    // this is essentially how much light reaches the current sample point due to scattering
                    float3 attn = exp(-beta_ray * (opt_i.x + opt_l.x) - beta_mie * (opt_i.y + opt_l.y) - beta_absorption * (opt_i.z + opt_l.z));

                    // accumulate the scattered light (how much will be scattered towards the camera)
                    total_ray += density.x * attn;
                    total_mie += density.y * attn;

                    // and increment the position on this ray
                    ray_pos_i += step_size_i;
                    
                }
                
                // calculate how much light can pass through the atmosphere
                float3 opacity = exp(-(beta_mie * opt_i.y + beta_ray * opt_i.x + beta_absorption * opt_i.z));
                
                // calculate and return the final color
                return (
                        phase_ray * beta_ray * total_ray // rayleigh color
                        + phase_mie * beta_mie * total_mie // mie
                        + opt_i.x * beta_ambient // and ambient
                ) * light_intensity + scene_color * opacity; // now make sure the background is rendered correctly
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float scene_depth_non_linear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float scene_depth;
                if (scene_depth_non_linear == 0.0f) {
                    // Do not clip with the far plane
                    scene_depth = 3.0f * _PlanetRadius;
                } else{
                    scene_depth = LinearEyeDepth(scene_depth_non_linear);
                }

                float3 planet_center = float3(0.0f, -_PlanetRadius, 0.0f);

                float3 ro = _WorldSpaceCameraPos;
                float3 rd = normalize(i.view_ray);

                float3 atmos_color = calculate_scattering(
                    ro,
                    rd,
                    scene_depth,
                    col,
                    normalize(_LightDir.xyz),
                    float3(_LightIntensity, _LightIntensity, _LightIntensity),
                    planet_center,
                    _PlanetRadius,
                    _PlanetRadius + _AtmosphereThickness,
                    _RayleighScattering * _RayleighScatteringScale,
                    _MieScatteringScale,
                    _OzoneAbsorption * _OzoneAbsorptionScale,
                    _AmbientScattering,
                    0.8,
                    _RayleighScaleHeight,
                    _MieScaleHeight,
                    _OzoneScaleHeight,
                    _AbsorptionFalloff,
                    _PrimarySteps,
                    _LightSteps
                );
     
                return fixed4(atmos_color, 1.0);
            }
            ENDCG
        }
    }
}
