import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { UserService } from '../../../user/services/user.service';

@Component({
    selector: 'company-unauthorized-dialog',
    templateUrl: './unauthorized-dialog.component.html',
    styleUrls: ['./unauthorized-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class UnauthorizedDialogComponent implements OnInit {

    public errorMessage: string;
    public info: string;
    public addInfo: string;
    public redirectUri: string;

    constructor(public dialogRef: MatDialogRef<UnauthorizedDialogComponent>,
                @Inject(MAT_DIALOG_DATA) public data: any, private _userService: UserService) {
        this.errorMessage = data.errorMessage;
        this.info = data.info;
        this.addInfo = data.addInfo;
        this.redirectUri = data.redirectUri;
    }

    ngOnInit() {
    }

    public closeDialog() {
        this.dialogRef.close();
    }

    public redirectToLogin() {
        this._userService.redirectToLogin(this.redirectUri);
    }

}
