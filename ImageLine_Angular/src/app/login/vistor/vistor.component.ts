import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Service } from 'src/app/shared/service';
import { Router } from '@angular/router';
import { LienceDto, LoginResultDto } from 'src/app/shared/dto';
import { NzMessageService } from 'ng-zorro-antd';

@Component({
  selector: 'app-vistor',
  templateUrl: './vistor.component.html',
  styleUrls: ['./vistor.component.css']
})
export class VistorComponent implements OnInit {

  form: FormGroup;
  needVisibility: boolean;
  loginResult: string;

  constructor(private fb: FormBuilder,
    private service: Service,
    private route: Router, 
    private message: NzMessageService,) {

    this.form = fb.group({
      userName: [null,],
      password: [null,],
    });
  }

  ngOnInit(): void {
  }

  vistor() {

    if(this.form.controls.userName.value == null 
      || this.form.controls.password.value == null
      || this.form.controls.userName.value == ""
      || this.form.controls.password.value == "")
      {
        this.message.warning("请输入账号密码！");
        return;
      }

    this.needVisibility = true;
    this.loginResult = "登陆中...";

    let userLoginDto = new LienceDto();
    userLoginDto.UserName = this.form.controls.userName.value;
    userLoginDto.License = this.form.controls.password.value;

    this.service.VisitorLogin(userLoginDto).subscribe(
      (data: LoginResultDto) => {
        if (data.IsSuccess) {
          localStorage.setItem("Token", data.Token);
          localStorage.setItem("IsLoginSuccess", "true");
          localStorage.setItem("UserId", data.UserID.toString());
          localStorage.setItem("IsUser", "false");
          this.needVisibility = false;
          this.route.navigate(['/showimage']);
        }
        else {
          this.needVisibility = true;
          this.loginResult = data.Reason;
        }
      },
      (error: any) => {
        this.message.error("网络发生异常 , 请重试！");
      }
    )
  }
}
