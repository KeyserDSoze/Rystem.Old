using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Rystem.Web
{
#pragma warning disable IDE1006 // Naming Styles
    public class CarouselComponent
    {
        public static CarouselComponent Default = new CarouselComponent()
        {
            navigation = new Navigation(),
            pagination = new Pagination()
        };
        public string direction => this.Direction.ToString().ToLower();
        [JsonIgnore]
        public CarouselDirectionType Direction { get; set; }
        /// <summary>
        /// Duration of transition between slides (in ms).
        /// </summary>
        [JsonProperty("speed")]
        public int Speed { get; set; } = 300;
        public string effect => this.Effect.ToString().ToLower();
        [JsonIgnore]
        public CarouselEffectType Effect { get; set; }
        /// <summary>
        /// Distance between slides in px.
        /// </summary>
        [JsonProperty("spaceBetween")]
        public int SpaceBetween { get; set; }
        public string slidesPerView => this.SlidesPerView > 0 ? this.SlidesPerView.ToString() : "auto";
        /// <summary>
        /// Number of slides per view (slides visible at the same time on slider's container).
        /// If you use it with "auto" value and along with loop: true then you need to specify loopedSlides parameter with amount of slides to loop (duplicate)
        /// </summary>
        [JsonIgnore]
        public int SlidesPerView { get; set; } = 1;
        /// <summary>
        /// Number of slides per column, for multirow layout.
        /// slidesPerColumn > 1 is currently not compatible with loop mode(loop: true)
        /// </summary>
        [JsonProperty("slidesPerColumn")]
        public int SlidesPerColumn { get; set; } = 1;
        /// <summary>
        /// Set numbers of slides to define and enable group sliding. Useful to use with slidesPerView > 1
        /// </summary>
        [JsonProperty("slidesPerGroup")]
        public int SlidesPerGroup { get; set; } = 1;
        [JsonProperty("resistance")]
        public bool Resistance { get; set; } 
        /// <summary>
        /// When enabled it won't allow to change slides by swiping or navigation/pagination buttons during transition
        /// </summary>
        [JsonProperty("preventInteractionOnTransition")]
        public bool PreventInteractionOnTransition { get; set; }
        /// <summary>
        /// Set to false to disable swiping to previous slide direction (to left or top)
        /// </summary>
        [JsonProperty("allowSlidePrev")]
        public bool AllowSlidePrev { get; set; } = true;
        /// <summary>
        /// Set to false to disable swiping to next slide direction (to right or bottom)
        /// </summary>
        [JsonProperty("allowSlideNext")]
        public bool AllowSlideNext { get; set; } = true;
        /// <summary>
        /// Allow 'swiper-no-swiping' class to not swipe items
        /// </summary>
        [JsonProperty("noSwiping")]
        public bool NoSwiping { get; set; }
        [JsonProperty("loop")]
        public bool Loop { get; set; }
        [JsonProperty("lazy")]
        public bool Lazy { get; set; }
        public Navigation navigation { get; set; }
        public Pagination pagination { get; set; }
        public Autoplay autoplay { get; set; }
        public FadeEffect fadeEffect { get; set; }
        public class Navigation
        {
            /// <summary>
            /// String with CSS selector or HTML element of the element that will work like "next" button after click on it
            /// </summary>
            [JsonProperty("nextEl")]
            public string NextEl { get; set; } = ".swiper-button-next";
            /// <summary>
            /// String with CSS selector or HTML element of the element that will work like "prev" button after click on it
            /// </summary>
            [JsonProperty("prevEl")]
            public string PrevEl { get; set; } = ".swiper-button-prev";
            /// <summary>
            /// Toggle navigation buttons visibility after click on Slider's container
            /// </summary>
            [JsonProperty("hideOnClick")]
            public bool HideOnClick { get; set; }
            /// <summary>
            /// CSS class name added to navigation button when it becomes disabled
            /// </summary>
            [JsonProperty("disabledClass")]
            public string DisabledClass { get; set; } 
            /// <summary>
            /// CSS class name added to navigation button when it becomes hidden
            /// </summary>
            [JsonProperty("hiddenClass")]
            public string HiddenClass { get; set; } 
        }
        public class Pagination
        {
            [JsonProperty("el")]
            public string Element { get; set; } = ".swiper-pagination";
            [JsonProperty("clickable")]
            public bool Clickable { get; set; } = true;
            public string type => this.Type.ToString().ToLower();
            [JsonIgnore]
            public PaginationType Type { get; set; }
            /// <summary>
            /// Good to enable if you use bullets pagination with a lot of slides. So it will keep only few bullets visible at the same time.
            /// </summary>
            [JsonProperty("dynamicBullets")]
            public bool DynamicBullets { get; set; }
            /// <summary>
            /// The number of main bullets visible when dynamicBullets enabled.
            /// </summary>
            [JsonProperty("dynamicMainBullets")]
            public int DynamicMainBullets { get; set; }
        }
        public class Scrollbar
        {
            [JsonProperty("hide")]
            public bool Hide { get; set; } = true;
        }
        public class Autoplay
        {
            /// <summary>
            /// Delay between transitions (in ms). If this parameter is not specified, auto play will be disabled
            /// </summary>
            [JsonProperty("delay")]
            public int Delay { get; set; } = 5000;
            /// <summary>
            /// Enable this parameter and autoplay will be stopped when it reaches last slide (has no effect in loop mode)
            /// </summary>
            [JsonProperty("stopOnLastSlide")]
            public bool StopOnLastSlide { get; set; }
            /// <summary>
            /// Set to false and autoplay will not be disabled after user interactions (swipes), it will be restarted every time after interaction
            /// </summary>
            [JsonProperty("disableOnInteraction")]
            public bool DisableOnInteraction { get; set; } = true;
            /// <summary>
            /// Enables autoplay in reverse direction
            /// </summary>
            [JsonProperty("reverseDirection")]
            public bool ReverseDirection { get; set; }
            /// <summary>
            /// When enabled autoplay will wait for wrapper transition to continue. Can be disabled in case of using Virtual Translate when your slider may not have transition
            /// </summary>
            [JsonProperty("waitForTransition")]
            public bool WaitForTransition { get; set; } = true;
        }
        public class FadeEffect
        {
            /// <summary>
            /// Enables slides cross fade
            /// </summary>
            [JsonProperty("crossFade")]
            public bool CrossFade { get; set; }
        }
    }
    public enum CarouselDirectionType
    {
        Horizontal,
        Vertical
    }
    public enum CarouselEffectType
    {
        Slide,
        Fade,
        Cube,
        Coverflow,
        Flip
    }
    public enum PaginationType
    {
        Bullets,
        Fraction,
        Progressbar
    }
#pragma warning restore IDE1006 // Naming Styles
}
