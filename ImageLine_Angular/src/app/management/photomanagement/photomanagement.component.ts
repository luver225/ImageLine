import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-photomanagement',
  templateUrl: './photomanagement.component.html',
  styleUrls: ['./photomanagement.component.css']
})
export class PhotomanagementComponent implements OnInit {

  isManagement:boolean;

  constructor() { }

  ngOnInit() {
    this.isManagement = true;
  }

}
