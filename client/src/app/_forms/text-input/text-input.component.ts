import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { text } from '@fortawesome/fontawesome-svg-core';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})
export class TextInputComponent implements ControlValueAccessor {

  constructor(@Self() public ngControl : NgControl) {
    this.ngControl.valueAccessor=this;
   }

  @Input() label : string;
  @Input() type = 'text';

  writeValue(obj: any): void {
  }

  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }
  

  

}
