import { User } from "./user";

export class UserParams {
  public  gender : string;
  public  minAge = 18;
  public   maxAge=99;
  public   pageNumber = 1;
  public  pageSize = 6;
  public orderBy = "lastActive";

    constructor(user : User){
        this.gender = user.gender === 'female' ? 'male' : 'female'
    }


}

