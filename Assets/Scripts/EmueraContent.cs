using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MinorShift.Emuera;
using MinorShift.Emuera.GameView;

public class EmueraContent : MonoBehaviour
{
    public static EmueraContent instance { get { return instance_; } }
    static EmueraContent instance_ = null;

    public string default_fontname;
    public Text template_text;
    public Image template_block;
    public RectTransform template_images;
    public RectTransform image_content;
    public RectTransform text_content;
    public RectTransform cache_images;
    public OptionWindow option_window;

    Camera main_camere;
    Image background;
    uEmuera.Drawing.Color background_color;

    public RectTransform rect_transform { get { return (RectTransform)transform; } }
    RectMask2D mask2d;

    void Awake()
    {
        FontUtils.SetDefaultFont(default_fontname);
        main_camere = GameObject.FindObjectOfType<Camera>();
    }

    void Start()
    {
        instance_ = this;
        background = GetComponent<Image>();
        mask2d = GetComponent<RectMask2D>();

        GenericUtils.SetListenerOnBeginDrag(gameObject, OnBeginDrag);
        GenericUtils.SetListenerOnDrag(gameObject, OnDrag);
        GenericUtils.SetListenerOnEndDrag(gameObject, OnEndDrag);
        GenericUtils.SetListenerOnClick(gameObject, OnClick);

        SetIntentBox(PlayerPrefs.GetInt("IntentBox_L", 0),
                    PlayerPrefs.GetInt("IntentBox_R", 0));
    }

    public void SetIntentBox(int left, int right)
    {
        if(left == 0 && right == 0)
            mask2d.enabled = false;
        else
            mask2d.enabled = true;
        rect_transform.anchoredPosition = new Vector2((left - right) / 2.0f, 0);
        rect_transform.sizeDelta = new Vector2(-right - left, 0);
    }

    int GetLineNoIndex(int lineno)
    {
        int high = end_index - 1;
        int low = begin_index;
        int mid = 0;
        int found = -1;

        while(low <= high)
        {
            mid = (low + high) / 2;
            int k = console_lines_[mid % max_log_count].LineNo;
            if(k > lineno)
                high = mid - 1;
            else if(k < lineno)
                low = mid + 1;
            else
            {
                found = mid;
                break;
            }
        }

        if(found < 0)
            return -1;
        return found;
    }

    int GetLineNoIndexForPosY(float y)
    {
        int high = end_index - 1;
        int low = begin_index;
        int mid = 0;
        while(low <= high)
        {
            mid = (low + high) / 2;
            var l = console_lines_[mid % max_log_count];
            float top = l.position_y;
            float bottom = l.position_y + l.height;
            if(y < top)
                high = mid - 1;
            else if(y > bottom)
                low = mid + 1;
            else
                return mid;
        }
        if(high <= begin_index)
            return begin_index;
        else if(low >= end_index - 1)
            return end_index - 1;
        return -1;
    }

    int GetPrevLineNoIndex(int index)
    {
        if(index > max_index || index < 0)
            return -1;

        var lineno = 0;
        var cindex = index;
        var zero = begin_index;

        if(!console_lines_[index % max_log_count].IsLogicalLine)
        {
            lineno = console_lines_[index % max_log_count].LineNo;
            for(; cindex >= zero; --cindex)
            {
                if(console_lines_[cindex % max_log_count].LineNo != lineno)
                    break;
            }
            return cindex + 1;
        }
        else
            return cindex;
    }

    int GetNextLineNoIndex(int index)
    {
        if(index > max_index || index < 0)
            return -1;

        var lineno = console_lines_[index % max_log_count].LineNo;
        var cindex = index;
        for(; cindex < max_index; ++cindex)
        {
            if(console_lines_[cindex % max_log_count].LineNo != lineno)
                break;
        }
        return cindex - 1;
    }

    public void Update()
    {
        if(!dirty && drag_delta == Vector2.zero)
            return;
        dirty = false;

        float display_width = DISPLAY_WIDTH;
        float display_height = DISPLAY_HEIGHT;

        if(drag_delta != Vector2.zero)
        {
            float t = drag_delta.magnitude;
            drag_delta *= (Mathf.Max(0, t - 300.0f * Time.deltaTime) / t);
            local_position = GetLimitPosition(local_position + drag_delta,
                                            display_width, display_height);
            if((local_position.x <= display_width - content_width && drag_delta.x < 0) ||
                (local_position.x >= 0 && drag_delta.x > 0))
                drag_delta.x = 0;
            if((local_position.y >= content_height - display_height && drag_delta.y > 0) ||
                (local_position.y <= offset_height && drag_delta.y < 0))
                drag_delta.y = 0;
        }

        var pos = local_position + (drag_curr_position - drag_begin_position);
        pos = GetLimitPosition(pos, display_width, display_height);

        int remove_count = 0;
        int count = display_lines_.Count;
        int max_line_no = -1;
        int min_line_no = int.MaxValue;
        for(int i = 0; i < count - remove_count; ++i)
        {
            var line = display_lines_[i];
            if(line.logic_y > pos.y + display_height ||
                line.logic_y + line.logic_height < pos.y)
            {
                display_lines_[i] = display_lines_[count - remove_count - 1];
                PushLine(line);
                ++remove_count;
                --i;
            }
            else
            {
                line.SetPosition(pos.x + line.unit_desc.posx, pos.y - line.logic_y);
                max_line_no = System.Math.Max(line.LineNo, max_line_no);
                min_line_no = System.Math.Min(line.LineNo, min_line_no);
            }
        }
        if(remove_count > 0)
            display_lines_.RemoveRange(count - remove_count, remove_count);

        if (image_removelist == null)
            image_removelist = new List<EmueraImage>();
        var display_iter = display_images_.GetEnumerator();
        while(display_iter.MoveNext())
        {
            var image = display_iter.Current.Value;
            if(image.logic_y > pos.y + display_height ||
                image.logic_y + image.logic_height < pos.y)
            {
                image_removelist.Add(image);
            }
            else
                image.SetPosition(pos.x + image.unit_desc.posx, pos.y - image.logic_y);
        }
        if(image_removelist.Count > 0)
        {
            var listcount = image_removelist.Count;
            EmueraImage image = null;
            for(int i=0; i<listcount; ++i)
            {
                image = image_removelist[i];
                PushImageContainer(image);
                display_images_.Remove(image.LineNo * 1000 + image.UnitIdx);
            }
            image_removelist.Clear();
        }

        var index = GetLineNoIndex(min_line_no - 1);
        index = GetPrevLineNoIndex(index);
        if(index >= 0)
        {
            UpdateLine(pos, display_height, index, -1);
        }
        index = GetLineNoIndex(max_line_no + 1);
        index = GetNextLineNoIndex(index);
        if(index >= 0)
        {
            UpdateLine(pos, display_height, index, +1);
        }
        if(display_lines_.Count == 0 &&
            console_lines_ != null && console_lines_.Count > 0)
        {
            index = GetLineNoIndexForPosY(pos.y);
            UpdateLine(pos, display_height, index, -1);
            UpdateLine(pos, display_height, index + 1, +1);
        }
    }
    void UpdateLine(Vector2 local, float display_height, int index, int delta)
    {
        var zero = begin_index;
        var max = end_index;
        while(index >= zero && index < max)
        {
            var line_desc = console_lines_[index % max_log_count];
            var y = line_desc.position_y;
            if(y > local.y + display_height || y + line_desc.height < local.y)
                break;
            var line = PullLine();
            line.logic_y = y;
            line.logic_height = line_desc.height;
            line.line_desc = line_desc;
            line.UnitIdx = 0; // Set appropriate unit index
            if (line_desc.units != null && line_desc.units.Count > 0)
                line.Width = line_desc.units[0].width; // Set width from first unit
            else
                line.Width = 0;
            line.UpdateContent();
            display_lines_.Add(line);
            line.SetPosition(local.x + line.unit_desc.posx, local.y - y);
            
            // Handle images if they exist
            if (line_desc.units != null && line_desc.units.Count > 0)
            {
                var image_indices = line_desc.units[0].image_indices;
                if(image_indices != null && image_indices.Count > 0)
                {
                    var image = PullImageContainer();
                    image.logic_y = y;
                    image.logic_height = line_desc.height;
                    image.line_desc = line_desc;
                    image.UnitIdx = 0;
                    image.Width = line_desc.units[0].width;
                    image.UpdateContent();
                    image.SetPosition(local.x + image.unit_desc.posx, local.y - y);
                    display_images_[image.LineNo * 1000 + 0] = image;
                }
            }
            index += delta;
        }
    }

    Vector2 GetLimitPosition(Vector2 local,
        float display_width, float display_height)
    {
        if (content_width < display_width)
            local.x = 0;
        else if (local.x > 0)
            local.x = 0;
        else if (local.x < display_width - content_width)
            local.x = display_width - content_width;

        if (content_height < display_height)
        {
            if (offset_height < 0)
                local.y = 0;
            else
                local.y = offset_height;
        }
        else
        {
            if (local.y < offset_height)
                local.y = offset_height;
            else if (local.y > content_height - display_height)
                local.y = content_height - display_height;
        }
        return local;
    }

    List<EmueraImage> image_removelist;

    public void SetDirty()
    {
        dirty = true;
    }
    bool dirty = false;
    uint last_click_tic = 0;

    void OnBeginDrag(UnityEngine.EventSystems.PointerEventData e)
    {
        drag_begin_position = e.position;
        drag_delta = Vector2.zero;
    }

    void OnDrag(UnityEngine.EventSystems.PointerEventData e)
    {
        drag_curr_position = e.position;
    }

    void OnEndDrag(UnityEngine.EventSystems.PointerEventData e)
    {
        drag_delta = e.position - drag_begin_position;
        local_position += drag_curr_position - drag_begin_position;
        local_position = GetLimitPosition(local_position, DISPLAY_WIDTH, DISPLAY_HEIGHT);
        drag_begin_position = Vector2.zero;
        drag_curr_position = Vector2.zero;
    }

    void OnClick()
    {
        //Debug.Log("OnClick");
    }

    Vector2 drag_begin_position = Vector2.zero;
    Vector2 drag_curr_position = Vector2.zero;
    Vector2 drag_delta = Vector2.zero;

    public void SetBackgroundColor(uEmuera.Drawing.Color color)
    {
        background_color = color;
        if(color.A == 0)
            background.enabled = false;
        else
            background.enabled = true;
        background.color = new Color32(color.R, color.G, color.B, color.A);
    }

    public void Ready()
    {
        console_lines_ = new List<EmueraBehaviour.LineDesc>();
        begin_index = 0;
        end_index = 0;
        max_index = 0;
        invalid_count = 0;
        offset_height = 0;
        content_width = 0;
        content_height = 0;
        local_position = Vector2.zero;
        last_button_generation = 0;
        while (cache_lines_.Count > 0)
            GameObject.Destroy(cache_lines_.Dequeue().gameObject);
        while (cache_image_containers_.Count > 0)
            GameObject.Destroy(cache_image_containers_.Pop().gameObject);
        while (cache_images_.Count > 0)
            GameObject.Destroy(cache_images_.Pop().gameObject);

        ready_ = true;
    }

    public void SetNoReady() { ready_ = false; }
    bool ready_ = false;

    public void Clear()
    {
        if (!ready_)
            return;
        for (int i = 0; i < display_lines_.Count; ++i)
            PushLine(display_lines_[i]);
        display_lines_.Clear();

        var display_iter = display_images_.GetEnumerator();
        while (display_iter.MoveNext())
            PushImageContainer(display_iter.Current.Value);
        display_images_.Clear();

        console_lines_.Clear();
        begin_index = 0;
        end_index = 0;
        max_index = 0;
        invalid_count = 0;
        offset_height = 0;
        content_width = 0;
        content_height = 0;
        local_position = Vector2.zero;
        last_button_generation = 0;

        SetDirty();
    }

    public void AddLine(object line, bool roll_to_bottom = false)
    {
        if (!ready_)
            return;
        var line_desc = (EmueraBehaviour.LineDesc)line;
        console_lines_.Add(line_desc);
        end_index++;

        var w = line_desc.posx + line_desc.width;
        if (content_width < w)
            content_width = w;

        line_desc.position_y = content_height;
        content_height += line_desc.height;
        max_index = console_lines_.Count;

        if (roll_to_bottom)
            ToBottom();
        SetDirty();
    }

    public object GetLine(int index)
    {
        if(index < 0 || index >= console_lines_.Count)
            return null;
        return console_lines_[index];
    }

    public int GetLineCount()
    {
        return console_lines_.Count;
    }

    public int GetMinLineNo()
    {
        if (console_lines_.Count == 0)
            return -1;
        return console_lines_[begin_index].LineNo;
    }

    public int GetMaxLineNo()
    {
        if (console_lines_.Count == 0)
            return -1;
        return console_lines_[end_index - 1].LineNo;
    }

    public void RemoveLine(int count)
    {
        if (!ready_)
            return;
        if (count == 0)
            return;
        if(count > valid_count)
            count = valid_count;

        var new_begin = begin_index + count;
        float remove_height = 0;
        int i;
        for(i=begin_index; i<new_begin; ++i)
        {
            var line = console_lines_[i % max_log_count];
            if (!line.IsLogicalLine)
                invalid_count--;
            remove_height += line.height;
        }

        begin_index = new_begin;
        offset_height += remove_height;

        //for (i = begin_index; i < end_index; ++i)
        //{
        //    var line = console_lines_[i % max_log_count];
        //    line.position_y -= remove_height;
        //}

        //if(begin_index > max_log_count && end_index > max_log_count)
        //{
        //    begin_index -= max_log_count;
        //    end_index -= max_log_count;
        //}
    }

    public void ToBottom()
    {
        var display_height = DISPLAY_HEIGHT;
        if (content_height < display_height)
        {
            if (offset_height < 0)
                local_position = new Vector2(local_position.x, 0);
            else
                local_position = new Vector2(local_position.x, offset_height);
        }
        else
            local_position = new Vector2(local_position.x, content_height - display_height);
    }

    public void ShowIsInProcess(bool value)
    {
        option_window.ShowIsInProcess(value);
    }

    public void SetLastButtonGeneration(int generation)
    {
        if (last_button_generation == generation)
            return;
        last_button_generation = generation;

        var display_iter = display_images_.GetEnumerator();
        while (display_iter.MoveNext())
        {
            var image = display_iter.Current.Value;
            var button = image.GetComponent<EmueraButton>();
            if (button != null)
                button.SetGray(true);
        }
    }

    public int button_generation { get { return last_button_generation; } }
    
    //public float Scale { get; private set; }

    int last_button_generation = 0;
    int max_index = 0;
    int invalid_count = 0;
    int begin_index
    {
        get { return console_lines_.Count > max_log_count ? console_lines_.Count - max_log_count : 0; }
        set { }
        //get { return begin_index_; }
        //set { begin_index_ = value; }
    }
    int end_index
    {
        get { return console_lines_.Count; }
        set { }
        //get { return end_index_; }
        //set { end_index_ = value; }
    }
    int valid_count
    {
        get { return end_index - begin_index - invalid_count; }
    }
    //int begin_index_ = 0;
    //int end_index_ = 0;

    public int max_log_count { get { return MinorShift.Emuera.Config.MaxLog; } }
    List<EmueraBehaviour.LineDesc> console_lines_;

    //public void SetScale(float scale)
    //{
    //    Scale = scale;
    //    rect_transform.localScale = new Vector3(scale, scale, 1);
    //}
    
    float DISPLAY_WIDTH { get { return rect_transform.rect.width; } }
    float DISPLAY_HEIGHT { get { return rect_transform.rect.height; } }

    //public float ScaledDisplayHeight { get { return rect_transform.rect.height / Scale; } }

    float offset_height = 0;

    float content_width = 0;

    float content_height = 0;

    Vector2 local_position = Vector2.zero;

    List<EmueraLine> display_lines_ = new List<EmueraLine>();
    Dictionary<int, EmueraImage> display_images_ = new Dictionary<int, EmueraImage>();
    //Dictionary<string, Sprite> sprite_cache_ = new Dictionary<string, Sprite>();

    
    EmueraLine PullLine()
    {
        if (cache_lines_.Count > 0)
        {
            var line = cache_lines_.Dequeue();
            line.gameObject.SetActive(true);
            return line;
        }
        else
        {
            var text = (Text)GameObject.Instantiate(template_text);
            text.transform.SetParent(text_content, false);
            var line = text.gameObject.AddComponent<EmueraLine>();
            return line;
        }
    }

    void PushLine(EmueraLine line)
    {
        line.gameObject.SetActive(false);
        var button = line.GetComponent<EmueraButton>();
        if(button != null)
            GameObject.Destroy(button);
        cache_lines_.Enqueue(line);
    }
    
    Queue<EmueraLine> cache_lines_ = new Queue<EmueraLine>();


    EmueraImage PullImageContainer()
    {
        if(cache_image_containers_.Count > 0)
        {
            var image = cache_image_containers_.Pop();
            image.gameObject.SetActive(true);
            return image;
        }
        else
        {
            var image_transform = (RectTransform)GameObject.Instantiate(template_images);
            image_transform.SetParent(image_content, false);
            var image = image_transform.gameObject.AddComponent<EmueraImage>();
            return image;
        }
    }
    void PushImageContainer(EmueraImage image)
    {
        image.gameObject.SetActive(false);
        var button = image.GetComponent<EmueraButton>();
        if (button != null)
            GameObject.Destroy(button);
        cache_image_containers_.Push(image);
    }
    Stack<EmueraImage> cache_image_containers_ = new Stack<EmueraImage>();

    public Image PullImage()
    {
        if (cache_images_.Count > 0)
        {
            var image = cache_images_.Pop();
            image.gameObject.SetActive(true);
            return image;
        }
        else
        {
            var image_go = new GameObject();
            var image = image_go.AddComponent<Image>();
            return image;
        }
    }
    public void PushImage(Image image)
    {
        if(image.gameObject.transform.parent != cache_images)
            image.gameObject.transform.SetParent(cache_images, false);
        image.gameObject.SetActive(false);
        cache_images_.Push(image);
    }
    Stack<Image> cache_images_ = new Stack<Image>();
}
