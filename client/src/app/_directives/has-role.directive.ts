import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { map, take } from 'rxjs/operators';
import { User } from '../models/user';
import { AccountService } from '../services/account.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole : string[];
  user : User;

  constructor(private accountService : AccountService,
    private viewContainerRef : ViewContainerRef,
    private templateRef : TemplateRef<any>
    ) { 
      this.accountService.currentUser$.pipe(take(1)).subscribe(
        user => {this.user = user; }
      )
    }
  ngOnInit(): void {
      if(this.user == null || !this.user?.roles) {
      this.viewContainerRef.clear();
      return;
    }

    if(this.user?.roles.some(r => this.appHasRole.includes(r))){
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    }
    else{
      this.viewContainerRef.clear();

    }

  }

}
